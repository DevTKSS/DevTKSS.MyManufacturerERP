using System.Text.Json;
using System.Text.RegularExpressions;
using Temp.Extensibility.DesktopAuthBroker;
using Windows.Security.Authentication.Web;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

/// <summary>
/// Service for handling OAuth authentication flows, specifically for Etsy API integration.
/// Uses HttpListenerServer for desktop OAuth flow.
/// </summary>
public partial class OAuthService : IAuthenticationService, IDisposable
{
    [GeneratedRegex(@"^(\d+)\.")]
    private static partial Regex DoesContainUserId();

    private const string EtsyOAuthProvider = "EtsyOAuth";

    private readonly IEtsyOAuthEndpoints _authEndpointsClient;
    private readonly IEtsyUserEndpoints _userEndpointsClient;
    private readonly HttpListenerServer _httpListenerServer;
    private readonly IOptions<OAuthOptions> _options;
    private readonly ITokenCache _tokenCache;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IEtsyOAuthEndpoints authEndpointsClient,
        IEtsyUserEndpoints userEndpointsClient,
        HttpListenerServer httpListenerServer,
        IOptions<OAuthOptions> options,
        ITokenCache tokenCache,
        ILogger<OAuthService> logger)
    {
        _authEndpointsClient = authEndpointsClient ?? throw new ArgumentNullException(nameof(authEndpointsClient));
        _userEndpointsClient = userEndpointsClient ?? throw new ArgumentNullException(nameof(userEndpointsClient));
        _httpListenerServer = httpListenerServer ?? throw new ArgumentNullException(nameof(httpListenerServer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var optionsValue = _options.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(optionsValue?.ClientID, nameof(optionsValue.ClientID));
        ArgumentOutOfRangeException.ThrowIfLessThan(optionsValue?.Scopes?.Length ?? 0, 1, nameof(optionsValue.Scopes));
    }

    public string[] Providers => new[] { EtsyOAuthProvider };

    public event EventHandler? LoggedOut;

    public async ValueTask<bool> IsAuthenticated(CancellationToken? cancellationToken = null)
    {
        try
        {
            var tokens = await _tokenCache.GetAsync(cancellationToken ?? CancellationToken.None);
            if (tokens == null) return false;

            if (tokens.TryGetAccessToken(out var accessToken) && !string.IsNullOrWhiteSpace(accessToken))
            {
                // Check if token is still valid (not expired)
                if (tokens.TryGetExpirationDate(out var expiry) && expiry > DateTime.Now.AddMinutes(5))
                {
                    return true;
                }

                // Try to refresh if we have a refresh token
                return await RefreshAsync(cancellationToken);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }

    public async ValueTask<bool> LoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials = null, string? provider = null, CancellationToken? cancellationToken = null)
    {
        if (provider != null && provider != EtsyOAuthProvider)
        {
            _logger.LogWarning("Unsupported provider: {Provider}", provider);
            return false;
        }

        try
        {
            _logger.LogInformation("Starting OAuth login flow using HttpListenerServer");

            // Get callback URI from the server
            var callbackUri = _httpListenerServer.GetCurrentApplicationCallbackUri();
            
            // Create authorization request URI  
            var authUrl = BuildAuthorizationUrl(callbackUri.ToString());
            var requestUri = new Uri(authUrl);
            
            _logger.LogDebug("Starting authentication with auth URL: {AuthUrl}", authUrl);
            
            // Use HttpListenerServer for authentication flow
            var result = await _httpListenerServer.AuthenticateAsync(
                WebAuthenticationOptions.None, 
                requestUri, 
                callbackUri, 
                cancellationToken ?? CancellationToken.None);

            if (result.ResponseStatus != WebAuthenticationStatus.Success)
            {
                _logger.LogWarning("OAuth authentication failed with status: {Status}", result.ResponseStatus);
                return false;
            }

            // Extract authorization code from the result
            var authCode = ExtractAuthorizationCodeFromResult(result.ResponseData);
            
            if (string.IsNullOrWhiteSpace(authCode))
            {
                _logger.LogWarning("No authorization code received from authentication result");
                return false;
            }

            // Exchange code for tokens
            var tokenResponse = await ExchangeCodeForTokensAsync(authCode, callbackUri.ToString());
            
            if (tokenResponse == null)
            {
                _logger.LogError("Failed to exchange authorization code for tokens");
                return false;
            }

            // Store tokens
            var tokens = new Dictionary<string, string>
            {
                [OAuthTokenRefreshDefaults.AccessTokenKey] = tokenResponse.AccessToken!,
                [OAuthTokenRefreshDefaults.RefreshToken] = tokenResponse.RefreshToken!,
                [OAuthTokenRefreshExtendedDefaults.ExpirationDateKey] = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToString("g")
            };

            await _tokenCache.SaveAsync(EtsyOAuthProvider, tokens, cancellationToken ?? CancellationToken.None);
            
            _logger.LogInformation("OAuth login completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth login failed");
            return false;
        }
    }

    public async ValueTask<bool> LogoutAsync(IDispatcher? dispatcher, CancellationToken? cancellationToken = null)
    {
        try
        {
            await _tokenCache.ClearAsync(cancellationToken ?? CancellationToken.None);
            LoggedOut?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("User logged out successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    public async ValueTask<bool> RefreshAsync(CancellationToken? cancellationToken = null)
    {
        try
        {
            var tokens = await _tokenCache.GetAsync(cancellationToken ?? CancellationToken.None);
            if (tokens == null || !tokens.TryGetRefreshToken(out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("No refresh token available");
                return false;
            }

            var tokenResponse = await _authEndpointsClient.RefreshTokenAsync(new RefreshTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.RefreshToken,
                ClientId = _options.Value.ClientID!,
                RefreshToken = refreshToken
            });

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken) || string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                _logger.LogError("Token refresh response missing required tokens");
                return false;
            }

            // Update stored tokens using dictionary indexer
            tokens[OAuthTokenRefreshDefaults.AccessTokenKey] = tokenResponse.AccessToken;
            tokens[OAuthTokenRefreshDefaults.RefreshToken] = tokenResponse.RefreshToken;
            tokens[OAuthTokenRefreshExtendedDefaults.ExpirationDateKey] = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToString("g");

            await _tokenCache.SaveAsync(EtsyOAuthProvider, tokens, cancellationToken ?? CancellationToken.None);
            
            _logger.LogInformation("Tokens refreshed successfully");
            return true;
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error during token refresh: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing tokens");
            return false;
        }
    }

    private string BuildAuthorizationUrl(string redirectUri)
    {
        var options = _options.Value;
        var baseUrl = options.AuthorizationEndpoint ?? "https://openapi.etsy.com/v3/public/oauth/connect";
        
        // Generate PKCE values
        var state = OAuth2Utilitys.GenerateState();
        var codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier);
        
        var url = new UriBuilder(baseUrl);
        var queryParams = new StringBuilder();
        
        void AddParam(string key, string value)
        {
            if (queryParams.Length > 0) queryParams.Append('&');
            queryParams.Append(Uri.EscapeDataString(key))
                      .Append('=')
                      .Append(Uri.EscapeDataString(value));
        }

        AddParam(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.CodeKey);
        AddParam(OAuthAuthRequestDefaults.ClientIdKey, options.ClientID!);
        AddParam(OAuthAuthRequestDefaults.RedirectUriKey, redirectUri);
        AddParam(OAuthAuthRequestDefaults.ScopeKey, string.Join(" ", options.Scopes!));
        AddParam(OAuthAuthRequestDefaults.StateKey, state);
        AddParam(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
        AddParam(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

        url.Query = queryParams.ToString();
        return url.Uri.ToString();
    }

    private string? ExtractAuthorizationCodeFromResult(string responseData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(responseData)) return null;
            
            // Parse the response URL to extract the code parameter
            var uri = new Uri(responseData);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["code"];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting authorization code from response: {ResponseData}", responseData);
            return null;
        }
    }

    private async Task<TokenResponse?> ExchangeCodeForTokensAsync(string authCode, string redirectUri)
    {
        try
        {
            var tokenResponse = await _authEndpointsClient.ExchangeCodeAsync(new AccessTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.AuthorizationCode,
                ClientId = _options.Value.ClientID!,
                RedirectUri = redirectUri,
                Code = authCode,
                CodeVerifier = string.Empty // HttpListenerServer handles PKCE internally
            });

            return tokenResponse;
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error during code exchange: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            
            // Try to parse error response
            if (!string.IsNullOrWhiteSpace(apiEx.Content))
            {
                try
                {
                    using var doc = JsonDocument.Parse(apiEx.Content);
                    if (doc.RootElement.TryGetProperty("error", out var errProp))
                    {
                        _logger.LogError("OAuth error: {Error}", errProp.GetString());
                    }
                    if (doc.RootElement.TryGetProperty("error_description", out var descProp))
                    {
                        _logger.LogError("OAuth error description: {Description}", descProp.GetString());
                    }
                }
                catch (JsonException)
                {
                    _logger.LogError("Failed to parse error response: {Content}", apiEx.Content);
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code for tokens");
            return null;
        }
    }

    public void Dispose()
    {
        // HttpListenerServer doesn't require disposal
    }
}