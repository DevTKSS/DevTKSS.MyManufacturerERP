

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

/// <summary>
/// Service for handling OAuth authentication flows, specifically for Etsy API integration.
/// Uses HttpListenerServer for desktop OAuth flow.
/// </summary>
public partial class OAuthService : IOAuthService
//: IAuthenticationService
{
    [GeneratedRegex(@"^(\d+)\.")]
    private static partial Regex DoesContainUserId();

    public const string ProviderName = "EtsyOAuth";

    private readonly IEtsyOAuthEndpoints _authEndpointsClient;
    private readonly IEtsyUserEndpoints _userEndpointsClient;
    private readonly OAuthOptions _options;
    private readonly ITokenCache _tokenCache;
    private readonly IServiceProvider _serviceProvider;

    private string? _codeVerifier;
    private string? _state;
    public OAuthService(
        IServiceProvider serviceProvider,
        IEtsyOAuthEndpoints authEndpointsClient,
        IEtsyUserEndpoints userEndpointsClient,
        IOptions<OAuthOptions> options,
        ITokenCache tokenCache,
        string providerName
        )
    {
        _authEndpointsClient = authEndpointsClient ?? throw new ArgumentNullException(nameof(authEndpointsClient));
        _userEndpointsClient = userEndpointsClient ?? throw new ArgumentNullException(nameof(userEndpointsClient));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
        _serviceProvider = serviceProvider;
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ClientID, nameof(_options.ClientID));
        ArgumentOutOfRangeException.ThrowIfLessThan(_options.Scopes?.Length ?? 0, 1, nameof(_options.Scopes));
    }

    public string[] Providers => new[] { ProviderName };

    public event EventHandler? LoggedOut;

    public async ValueTask<bool> IsAuthenticated(CancellationToken? cancellationToken = default)
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
                var refreshedTokens = await RefreshAsync(_serviceProvider, await _tokenCache.GetAsync(cancellationToken ?? CancellationToken.None), cancellationToken);
                return await _tokenCache.GetCurrentProviderAsync(cancellationToken ?? CancellationToken.None) == ProviderName
                             && refreshedTokens?.Count > 0;
            }

            return false;
        }
        catch (Exception ex)
        {
           Log.Error(ex, "Error checking authentication status");
            return false;
        }
    }

    public async ValueTask<IDictionary<string, string>?> LoginAsync(
        IServiceProvider serviceProvider,
        IDispatcher? dispatcher,
        ITokenCache tokenCache,
        IDictionary<string, string> tokens,
        // string? provider = null,
        CancellationToken? cancellationToken = default)
    {
        //if (provider is null || !Providers.Contains(provider))
        //{
        //    _logger.LogWarning("Unsupported provider: {Provider}", provider);
        //    return false;
        //}
        var userEndpointsClient = serviceProvider.GetRequiredService<IEtsyUserEndpoints>();
        var httpListenerServer = serviceProvider.GetRequiredService<IHttpListenerService>();
        try
        {
            Log.Information("Starting OAuth login flow using HttpListenerServer");

            // Get callback URI from the server
            var callbackUri = httpListenerServer.GetCurrentApplicationCallbackUri();

            // Create authorization request URI  
            var authUrl = PrepareLoginStartUri(callbackUri.ToString());
            var requestUri = new Uri(authUrl);

            Log.Debug("Starting authentication with auth URL: {AuthUrl}", authUrl);

            // Use HttpListenerServer for authentication flow
            var result = await httpListenerServer.AuthenticateAsync(
                WebAuthenticationOptions.None,
                requestUri,
                callbackUri,
                cancellationToken ?? CancellationToken.None);

            if (result.ResponseStatus != WebAuthenticationStatus.Success
                || result.ResponseData is null)
            {
                Log.Warning("OAuth authentication failed with status: {Status}", result.ResponseStatus);
                return default;
            }

            var queryParams = GetQuery(result.ResponseData);
            //if (string.IsNullOrWhiteSpace(authCode))
            //{
            //    _logger.LogWarning("No authorization code received from authentication result");
            //    return false;
            //}
            //if(string.IsNullOrWhiteSpace(_codeVerifier))
            //{
            //    _logger.LogWarning("PKCE code verifier is missing");
            //    return false;
            //}
            //// Exchange code for tokens
            //var tokenResponse = await ExchangeCodeForTokensAsync(authCode,_codeVerifier, callbackUri.ToString());

            //if (tokenResponse == null)
            //{
            //    _logger.LogError("Failed to exchange authorization code for tokens");
            //    return false;
            //}

            if (int.TryParse(queryParams[OAuthTokenRefreshDefaults.ExpiresInKey], out var expiresIn) == false)
            {
                expiresIn = 3600;
            }
            if (queryParams.Get(OAuthTokenRefreshDefaults.AccessTokenKey) is not string accessToken ||
                queryParams.Get(OAuthTokenRefreshDefaults.RefreshToken) is not string refreshToken)
            {
                Log.Error("Token response missing required tokens");
                return default;
            }

            // Store tokens
            tokens.AddRange(new Dictionary<string, string>
            {
                [OAuthTokenRefreshDefaults.AccessTokenKey] = accessToken,
                [OAuthTokenRefreshDefaults.RefreshToken] = refreshToken,
                [OAuthTokenRefreshExtendedDefaults.ExpirationDateTokenKey] = DateTime.Now.AddSeconds(expiresIn).ToString("g")

            });
            if (DoesContainUserId().Match(refreshToken) is { Success: true, Groups.Count: > 1 } match)
            {
                var userId = match.Groups[1].Value;
                Log.Information("Logged in as user ID: {UserId}", userId);
                tokens.AddOrReplace(OAuthTokenRefreshExtendedDefaults.UserIdKey, userId);
            }
            var meResponse = await userEndpointsClient.GetMeAsync(accessToken, _options.ClientID!);
            if (meResponse.ShopId == 0)
            {
                throw new InvalidOperationException("No shop associated with this user. Can not fetch data from this.");
            }
            tokens.AddOrReplace($"{OAuthTokenRefreshExtendedDefaults.UserIdKey}_{EtsyOAuthMeRequestDefaults.ShopIdKey}", meResponse.ShopId.ToString());

            await tokenCache.SaveAsync(ProviderName, tokens, cancellationToken ?? CancellationToken.None);

            Log.Information("OAuth login completed successfully");
            return tokens;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "OAuth login failed");
            return default;
        }
    }

    public async ValueTask<bool> LogoutAsync(
        IDispatcher? dispatcher,
        IDictionary<string, string> tokens,
        CancellationToken cancellationToken = default)
    {
        try
        {
            LoggedOut?.Invoke(this, EventArgs.Empty);
            Log.Information("User logged out successfully");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during logout");
            return false;
        }
    }

    public async ValueTask<IDictionary<string, string>?> RefreshAsync(
        IServiceProvider serviceProvider,
        IDictionary<string, string> tokens,
        CancellationToken? cancellationToken = default)
    {
        var authEndpointsClient = serviceProvider.GetRequiredService<IEtsyOAuthEndpoints>();
        var options = serviceProvider.GetRequiredService<IOptions<OAuthOptions>>().Value;
        try
        {
            if (tokens == null || !tokens.TryGetRefreshToken(out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                Log.Warning("No refresh token available");
                return default;
            }

            var tokenResponse = await authEndpointsClient.RefreshTokenAsync(new RefreshTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.RefreshToken,
                ClientId = options.ClientID!,
                RefreshToken = refreshToken
            });

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken) || string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                Log.Error("Token refresh response missing required tokens");
                return default;
            }

            // Update stored tokens using dictionary indexer
            tokens[OAuthTokenRefreshDefaults.AccessTokenKey] = tokenResponse.AccessToken;
            tokens[OAuthTokenRefreshDefaults.RefreshToken] = tokenResponse.RefreshToken;
            tokens[OAuthTokenRefreshExtendedDefaults.ExpirationDateTokenKey] = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToString("g");

            Log.Information("Tokens refreshed successfully");
            return tokens;
        }
        catch (ApiException apiEx)
        {
            Log.Error(apiEx, "API error during token refresh: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            return default;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error refreshing tokens");
            return default;
        }
    }

    private string PrepareLoginStartUri(string redirectUri)
    {
        // Generate PKCE values
        _state = OAuth2Utilitys.GenerateState();
        _codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(_codeVerifier);

        var url = new UriBuilder(_options.AuthorizationEndpoint!);
        var queryParams = new StringBuilder();

        void AddParam(string key, string value)
        {
            if (queryParams.Length > 0) queryParams.Append('&');
            queryParams.Append(Uri.EscapeDataString(key))
                      .Append('=')
                      .Append(Uri.EscapeDataString(value));
        }

        AddParam(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.CodeKey);
        AddParam(OAuthAuthRequestDefaults.ClientIdKey, _options.ClientID!);
        AddParam(OAuthAuthRequestDefaults.RedirectUriKey, redirectUri);
        AddParam(OAuthAuthRequestDefaults.ScopeKey, string.Join(" ", _options.Scopes!));
        AddParam(OAuthAuthRequestDefaults.StateKey, _state);
        AddParam(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
        AddParam(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

        url.Query = queryParams.ToString();
        return url.Uri.ToString();
    }
    private NameValueCollection GetQuery(string redirectUri)
    {
        return redirectUri.StartsWith(_options.RedirectUri!)
             ? AuthHttpUtility.ExtractArguments(redirectUri)  // authData is a fully qualified url, so need to extract query or fragment
             : AuthHttpUtility.ParseQueryString(redirectUri.TrimStart('#').TrimStart('?')); // authData isn't full url, so just process as query or fragment

    }

    private async Task<TokenResponse?> ExchangeCodeForTokensAsync(string authCode, string codeVerifier, string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(authCode) || string.IsNullOrWhiteSpace(codeVerifier)) return null;
        try
        {
            var tokenResponse = await _authEndpointsClient.ExchangeCodeAsync(new AccessTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.AuthorizationCode,
                ClientId = _options.ClientID!,
                RedirectUri = redirectUri,
                Code = authCode,
                CodeVerifier = codeVerifier
            });

            return tokenResponse;
        }
        catch (ApiException apiEx)
        {
            Log.Error(apiEx, "API error during code exchange: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);

            // Try to parse error response
            if (!string.IsNullOrWhiteSpace(apiEx.Content))
            {
                try
                {
                    using var doc = JsonDocument.Parse(apiEx.Content);
                    if (doc.RootElement.TryGetProperty(OAuthErrorResponseDefaults.ErrorKey, out var errProp))
                    {
                        Log.Error("OAuth error: {Error}", errProp.GetString());
                    }
                    if (doc.RootElement.TryGetProperty(OAuthErrorResponseDefaults.ErrorDescriptionKey, out var descProp))
                    {
                        Log.Error("OAuth error description: {Description}", descProp.GetString());
                    }
                }
                catch (JsonException)
                {
                    Log.Error("Failed to parse error response: {Content}", apiEx.Content);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exchanging authorization code for tokens");
            return null;
        }
    }
}