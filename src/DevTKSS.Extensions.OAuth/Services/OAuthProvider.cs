using System.Net;
using System.Text.Json;
using DevTKSS.Extensions.OAuth.Endpoints;

namespace DevTKSS.Extensions.OAuth.Services;
/// <summary>
/// Service for handling OAuth authentication flows (desktop loopback via system browser).
/// </summary>
public partial record OAuthProvider : BaseAuthenticationProvider, IOAuthProvider
{
    [GeneratedRegex(@"^(\d+)\.")]
    public static partial Regex DoesContainUserId();

    public const string DefaultName = "OAuth";

    private protected string? _codeVerifier;
    private protected string? _state;
    private readonly ILogger _logger;
    private OAuthOptions? _options;
    private readonly IOAuthEndpoints _authEndpoints;
    private readonly ISystemBrowserAuthBrokerProvider _systemBrowser;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITokenCache _tokenCache;
    private OAuthSettings? _settings;

    public OAuthSettings Settings => _settings ?? new OAuthSettings();

    public OAuthProvider(
        ILogger providerLogger,
        IServiceProvider serviceProvider,
        IOAuthEndpoints authEndpoints,
        ISystemBrowserAuthBrokerProvider systemBrowser,
        ITokenCache tokenCache,
        string name = DefaultName) : base(providerLogger, name, Tokens: tokenCache)
    {
        _logger = providerLogger;
        _tokenCache = tokenCache;
        _serviceProvider = serviceProvider;
        _authEndpoints = authEndpoints;
        _systemBrowser = systemBrowser;
    }
    public void Configure(OAuthOptions options, OAuthSettings settings)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
    protected async override  ValueTask<IDictionary<string, string>?> InternalLoginAsync(
        IDispatcher? dispatcher,
        IDictionary<string, string>? tokens,
        CancellationToken cancellationToken)
    {

        if(_options is null)
        {
            _logger.LogError("OAuth options are not configured.");
            return default;
        }
        if (_options.EndpointOptions.RedirectUri is not string redirectUri || string.IsNullOrWhiteSpace(redirectUri)
            || !Uri.TryCreate(redirectUri, UriKind.Absolute, out Uri? callbackUri))
        {
            if (_systemBrowser.GetCurrentApplicationCallbackUri() is Uri appCallbackUri)
            {
                callbackUri = appCallbackUri;
                _logger.LogWarning("Using application callback URI: {CallbackUri}", callbackUri);
            }
            else
            {
                _logger.LogError("Redirect URI is not or not correctly configured.");
                return default;
            }
        }
        try
        {
            // Build a default start URI using options
            var defaultAuthUrl = PrepareLoginStartUri(callbackUri.ToString());
            // Allow app to override/prepare the start uri via settings
            if (Settings.PrepareLoginStartUri is not null)
            {
                defaultAuthUrl = await Settings.PrepareLoginStartUri(
                    _serviceProvider,
                    _tokenCache,
                    tokens,
                    defaultAuthUrl,
                    cancellationToken);
            }

            if(defaultAuthUrl is not string authUrl
                || string.IsNullOrWhiteSpace(authUrl))
            {
                _logger.LogError("Failed to prepare authorization URL.");
                return default;
            }
            var requestUri = new Uri(authUrl);

            _logger.LogDebug("Starting authentication with auth URL: {AuthUrl}", authUrl);

            var result = await _systemBrowser.AuthenticateAsync(
                WebAuthenticationOptions.None,
                requestUri,
                callbackUri!,
                cancellationToken);

            if (result.ResponseStatus != WebAuthenticationStatus.Success
                || result.ResponseData is null)
            {
                _logger.LogWarning("OAuth authentication failed with status: {Status}", result.ResponseStatus);
                return default;
            }
            return await InternalPostLogin(await _tokenCache.GetAsync(cancellationToken) ?? new Dictionary<string,string>(), result.ResponseData, cancellationToken);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth login failed");
            return default;
        }
    }

    public async ValueTask<IDictionary<string, string>?> InternalPostLogin(
        IDictionary<string, string> tokens,
        string redirectUri,
        CancellationToken cancellationToken)
    {

        if (_options?.EndpointOptions.RedirectUri is not string redirectUriConfig
            || string.IsNullOrWhiteSpace(redirectUriConfig))
        {
            _logger.LogError("Redirect URI is not configured.");
            return default;
        }
        var queryParams = redirectUri.GetQuery(redirectUriConfig);
        if (queryParams is null || queryParams.Count == 0)
        {
            _logger.LogWarning("No query parameters found in authentication result");
            return default;
        }
        var error = queryParams.Get(OAuthErrorResponseDefaults.ErrorKey);
        var errorDescription = queryParams.Get(OAuthErrorResponseDefaults.ErrorDescriptionKey);
        var errorUri = queryParams.Get(OAuthErrorResponseDefaults.ErrorUriKey);
        var authCode = queryParams.Get(OAuthAuthRequestDefaults.CodeKey);
        var returnedState = queryParams.Get(OAuthAuthRequestDefaults.StateKey);
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("OAuth error received: {Error}, Description: {Description}, Uri: {Uri}", error, errorDescription, errorUri);
            return default;
        }
        if (string.IsNullOrWhiteSpace(_state) || _state != returnedState)
        {
            _logger.LogWarning("State parameter mismatch. Potential CSRF attack.");
            return default;
        }
        if (string.IsNullOrWhiteSpace(authCode))
        {
            _logger.LogWarning("No authorization code received from authentication result");
            return default;
        }
        if (string.IsNullOrWhiteSpace(_codeVerifier))
        {
            _logger.LogWarning("PKCE code verifier is missing");
            return default;
        }
        // Exchange code for tokens
        var tokenResponse = await ExchangeCodeForTokensAsync(_serviceProvider, tokens, authCode, cancellationToken);

        if (tokenResponse is null
            || !tokenResponse.TryGetAccessToken(out var accessToken) || string.IsNullOrWhiteSpace(accessToken)
            || !tokenResponse.TryGetRefreshToken(out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogError("Failed to exchange authorization code for tokens");
            return default;
        }
        _logger.LogInformation("OAuth login completed successfully");

        // Allow post-login customization
        if (Settings.PostLoginCallback is not null)
        {
            var customized = await Settings.PostLoginCallback(
                _serviceProvider,
                _tokenCache,
                null,
                redirectUri,
                tokenResponse,
                cancellationToken);
            return customized ?? tokenResponse;
        }

        return tokenResponse;
        
    }

    protected override async ValueTask<IDictionary<string, string>?> InternalRefreshAsync(CancellationToken cancellationToken)
    {
        // Allow delegate override
        if (Settings.RefreshCallback is not null)
        {
            var current = await _tokenCache.GetAsync(cancellationToken) ?? new Dictionary<string,string>();
            var overridden = await Settings.RefreshCallback(
                _serviceProvider,
                _tokenCache,
                current,
                cancellationToken);
            if (overridden is not null)
            {
                return overridden;
            }
        }

        var tokens = await _tokenCache.GetAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(_options?.ClientID))
        {
            _logger.LogError("Client ID is not configured.");
            return null;
        }
        try
        {
            if (tokens == null || !tokens.TryGetRefreshToken(out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("No refresh token available");
                return default;
            }

            var tokenResponse = await _authEndpoints.RefreshTokenAsync(new RefreshTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.RefreshToken,
                ClientId = _options.ClientID,
                RefreshToken = refreshToken
            },cancellationToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken) || string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                _logger.LogError("Token refresh response missing required tokens");
                return default;
            }

            // Update stored tokens using dictionary indexer
            tokens[_options.TokenCacheKeyOptions.AccessTokenKey] = tokenResponse.AccessToken;
            tokens[_options.TokenCacheKeyOptions.RefreshTokenKey] = tokenResponse.RefreshToken;
            tokens[_options.TokenCacheKeyOptions.ExpirationDateKey] = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToString("g");

            _logger.LogInformation("Tokens refreshed successfully");
            return tokens;
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error during token refresh: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing tokens");
            return default;
        }
    }

    internal string? PrepareLoginStartUri(
        string? loginCallbackUri)
    {
        if (_options is null)
        {
            _logger.LogError("OAuth options are not configured.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(loginCallbackUri))
        {
            return null;
        }
        if (_options?.EndpointOptions.AuthorizationEndpoint is not string authEndpoint)
        {
           _logger.LogError("{AuthorizationEndpoint}: Authorization endpoint is not configured.", nameof(OAuthEndpointOptions.AuthorizationEndpoint));
            return null;
        }
        if (string.IsNullOrWhiteSpace(_options.ClientID))
        {
            _logger.LogError("{ClientID}: Client ID is not configured.", nameof(OAuthOptions.ClientID));
            return null;
        }
        if(_options.Scopes is null || _options.Scopes.Length == 0)
        {
            _logger.LogError("{Scopes}: At least one scope must be configured.", nameof(OAuthOptions.Scopes));
            return null;
        }
        // Generate PKCE values
        _state = OAuth2Utilitys.GenerateState();
        _codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(_codeVerifier);

        var url = new UriBuilder(authEndpoint);
        var queryParams = new StringBuilder();

        void AddParam(string key, string value)
        {
            if (queryParams.Length > 0) queryParams.Append('&');
            queryParams.Append(Uri.EscapeDataString(key))
                      .Append('=')
                      .Append(Uri.EscapeDataString(value));
        }

        AddParam(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.CodeKey);
        AddParam(OAuthAuthRequestDefaults.ClientIdKey, _options.ClientID);
        AddParam(OAuthAuthRequestDefaults.RedirectUriKey, loginCallbackUri);
        AddParam(OAuthAuthRequestDefaults.ScopeKey, string.Join(" ", _options.Scopes));
        AddParam(OAuthAuthRequestDefaults.StateKey, _state);
        AddParam(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
        AddParam(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

        url.Query = queryParams.ToString();
        return url.Uri.ToString();
    }

    protected async virtual ValueTask<IDictionary<string,string>?> ExchangeCodeForTokensAsync(
        IServiceProvider serviceProvider,
        IDictionary<string,string> tokens,
        string authCode,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(_options?.ClientID))
        {
            _logger.LogError("Client ID is not configured.");
            return null;
        }
        if(string.IsNullOrWhiteSpace(_codeVerifier))
        {
            _logger.LogError("PKCE code verifier is missing.");
            return null;
        }
        if (string.IsNullOrWhiteSpace(authCode))
        {   
            _logger.LogError("Authorization code is missing.");
            return null;
        }
        if (string.IsNullOrWhiteSpace(_options.EndpointOptions.RedirectUri))
        {
            _logger.LogError("Redirect URI is not configured.");
            return null;
        }
        try
        {
            var tokenResponse = await _authEndpoints.ExchangeCodeAsync(new AccessTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.AuthorizationCode,
                ClientId = _options.ClientID,
                RedirectUri = _options.EndpointOptions.RedirectUri,
                Code = authCode,
                CodeVerifier = _codeVerifier
            },cancellationToken);
            if (tokenResponse is null)
            {
                _logger.LogError("Token response is null");
                return default;
            }

            if (tokenResponse.AccessToken is not string accessToken
                || tokenResponse.RefreshToken is not string refreshToken
                || tokenResponse.TokenType is not string tokenType || tokenType != _options.TokenType)
            {
                _logger.LogError("Token response missing required tokens");
                return default;
            }

            // Store tokens
            tokens[_options.TokenCacheKeyOptions.AccessTokenKey] = accessToken;
            tokens[_options.TokenCacheKeyOptions.RefreshTokenKey] = refreshToken;
            tokens[_options.TokenCacheKeyOptions.ExpirationDateKey] = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToString("g");

            if (DoesContainUserId().Match(refreshToken!) is { Success: true, Groups.Count: > 1 } match)
            {
                var userId = match.Groups[1].Value;
                _logger.LogInformation("Logged in as user ID: {UserId}", userId);
                tokens.AddOrReplace(_options.TokenCacheKeyOptions.IdTokenKey, userId);
            }
            return tokens;
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
                    if (doc.RootElement.TryGetProperty(OAuthErrorResponseDefaults.ErrorKey, out var errProp))
                    {
                        _logger.LogError("OAuth error: {Error}", errProp.GetString());
                    }
                    if (doc.RootElement.TryGetProperty(OAuthErrorResponseDefaults.ErrorDescriptionKey, out var descProp))
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
}