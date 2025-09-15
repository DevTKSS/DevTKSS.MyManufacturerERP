using DevTKSS.Extensions.OAuth.Endpoints;
using Microsoft.Extensions.Configuration;
using static DevTKSS.Extensions.OAuth.Validation.UriValidationUtility;

namespace DevTKSS.Extensions.OAuth.OAuthServices;
/// <summary>
/// Service for handling OAuth authentication flows (desktop loopback via system browser).
/// </summary>
public partial record OAuthService(
        ILogger<OAuthService> ProviderLogger,
        IServiceProvider ServiceProvider,
        IOAuthEndpoints AuthEndpoints,
        ITokenCache Tokens,
        IOptionsSnapshot<OAuthOptions> Configuration,
        ISystemBrowserAuthBrokerProvider AuthBrowserProvider,
        [ServiceKey] string Name = OAuthService.DefaultName) : IOAuthService
{

    public OAuthSettings? Settings { get; init; }
    public const string DefaultName = "OAuth";

    private protected string? _codeVerifier;
    private protected string? _state;

    private OAuthSettings? _internalSettings;
    protected OAuthSettings InternalSettings
    {
        get
        {
            if (_internalSettings is null)
            {
                _internalSettings = Settings ?? new OAuthSettings();
                var options = Configuration.Get(Name);
                if (options is not null)
                {
                    _internalSettings = _internalSettings with
                    {
                        ClientOptions = options.ClientOptions ?? _internalSettings.ClientOptions ?? throw new ArgumentNullException("ClientOptions must be not null"),
                        LoginStartUri = !string.IsNullOrWhiteSpace(options.ClientOptions?.CallbackUri) ? options.ClientOptions?.CallbackUri : _internalSettings.LoginStartUri ?? AuthBrowserProvider.GetCurrentApplicationCallbackUri().OriginalString,
                        TokenCacheOptions = options.TokenCacheOptions ?? _internalSettings.TokenCacheOptions,
                        UriTokenOptions = options.UriTokenOptions ?? _internalSettings.UriTokenOptions,

                    };
                }
            }
            return _internalSettings;
        }
    }
    public async ValueTask<IDictionary<string, string>?> PostLoginAsync(
        IDictionary<string, string> tokens,
        string redirectUri,
        CancellationToken cancellationToken)
    {

        if (InternalSettings.LoginStartUri is not string redirectUriConfig
            || string.IsNullOrWhiteSpace(redirectUriConfig))
        {
            ProviderLogger.LogError("{loginStartUri} URI is not configured.", nameof(OAuthSettings.LoginStartUri));
            return default;
        }
        var queryParams = redirectUri.GetQuery(redirectUriConfig);
        if (queryParams is null || queryParams.Count == 0)
        {
            ProviderLogger.LogWarning("No query parameters found in authentication result");
            return default;
        }
        var error = queryParams.Get(OAuthErrorResponseDefaults.ErrorKey);
        var errorDescription = queryParams.Get(OAuthErrorResponseDefaults.ErrorDescriptionKey);
        var errorUri = queryParams.Get(OAuthErrorResponseDefaults.ErrorUriKey);
        var authCode = queryParams.Get(OAuthAuthRequestDefaults.CodeKey);
        var returnedState = queryParams.Get(OAuthAuthRequestDefaults.StateKey);
        if (!string.IsNullOrWhiteSpace(error))
        {
            ProviderLogger.LogWarning("OAuth error received: {Error}, Description: {Description}, Uri: {Uri}", error, errorDescription, errorUri);
            return default;
        }
        if (string.IsNullOrWhiteSpace(_state) || _state != returnedState)
        {
            ProviderLogger.LogWarning("State parameter mismatch. Potential CSRF attack.");
            return default;
        }
        if (string.IsNullOrWhiteSpace(authCode))
        {
            ProviderLogger.LogWarning("No authorization code received from authentication result");
            return default;
        }
        if (string.IsNullOrWhiteSpace(_codeVerifier))
        {
            ProviderLogger.LogWarning("PKCE code verifier is missing");
            return default;
        }
        // Exchange code for tokens
        var tokenResponse = await InternalExchangeCodeForTokensAsync(tokens, authCode, redirectUri, cancellationToken);

        ProviderLogger.LogInformation("OAuth post login finished");
        return tokenResponse;
    }

    public async ValueTask<IDictionary<string, string>?> RefreshAsync(
        CancellationToken cancellationToken)
    {

        var tokens = await Tokens.GetAsync(cancellationToken);
        if (InternalSettings.ClientOptions?.ClientID is not string clientID
            || string.IsNullOrWhiteSpace(clientID))
        {
            ProviderLogger.LogError("Client Options or Client ID is not configured.");
            return null;
        }
        try
        {
            if (tokens == null || !tokens.TryGetRefreshToken(out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                ProviderLogger.LogWarning("No refresh token available");
                return default;
            }

            var tokenResponse = await AuthEndpoints.RefreshTokenAsync(new RefreshTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.RefreshToken,
                ClientId = clientID,
                RefreshToken = refreshToken
            }, cancellationToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken) || string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                ProviderLogger.LogError("Token refresh response missing required tokens");
                return default;
            }

            tokens[InternalSettings.TokenCacheOptions.AccessTokenKey] = tokenResponse.AccessToken;
            tokens[InternalSettings.TokenCacheOptions.RefreshTokenKey] = tokenResponse.RefreshToken;
            tokens[InternalSettings.TokenCacheOptions.ExpiresInKey] = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToString("g");

            ProviderLogger.LogInformation("Tokens refreshed successfully");
            return tokens;
        }
        catch (ApiException apiEx)
        {
            ProviderLogger.LogError(apiEx, "API error during token refresh: {StatusCode} - {Content}", apiEx.StatusCode, apiEx.Content);
            return default;
        }
        catch (Exception ex)
        {
            ProviderLogger.LogError(ex, "Error refreshing tokens");
            return default;
        }
    }

    public async ValueTask<string> PrepareLoginStartUri(
        string? loginStartUri,
        CancellationToken token)
    {
        const string defaultReturn = "";
        if (InternalSettings is null)
        {
            ProviderLogger.LogError("OAuth Settings are not configured.");
            return defaultReturn;
        }

        if (string.IsNullOrWhiteSpace(loginStartUri))
        {
            return defaultReturn;
        }
        if (InternalSettings.ClientOptions is not OAuthClientOptions clientOptions)
        {
            ProviderLogger.LogError("{ClientOptions}: Client options are not configured.", nameof(OAuthClientOptions));
            return defaultReturn;
        }
        if (clientOptions.EndpointOptions?.AuthorizationEndpoint is not string authEndpoint
            || BeAValidUrl(authEndpoint))
        {
            ProviderLogger.LogError("{AuthorizationEndpoint}: Authorization endpoint is not configured.", nameof(OAuthEndpointOptions.AuthorizationEndpoint));
            return defaultReturn;
        }
        if (clientOptions.ClientID is not string clientID ||
            string.IsNullOrWhiteSpace(clientID))
        {
            ProviderLogger.LogError("{ClientID}: Client ID is not valid configured.", nameof(OAuthClientOptions.ClientID));
            return defaultReturn;
        }
        if (clientOptions.Scopes is not { } scopes
            || scopes.Length == 0)
        {
            ProviderLogger.LogError("{Scopes}: At least one scope must be configured and have at least one entry.", nameof(OAuthClientOptions.Scopes));
            return defaultReturn;
        }

        while (!token.IsCancellationRequested)
        {
            // Generate PKCE values
            _state = OAuth2Utilitys.GenerateState();
            _codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
            var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(_codeVerifier);

            var url = new UriBuilder(authEndpoint);
            var queryParams = new StringBuilder();

            void AddParam(string key, string value)
            {
                if (queryParams.Length > 0) queryParams.Append('&');
                queryParams.AppendFormat(Uri.EscapeDataString(key))
                          .Append('=')
                          .Append(Uri.EscapeDataString(value));
            }

            AddParam(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.CodeKey);
            AddParam(OAuthAuthRequestDefaults.ClientIdKey, clientID);
            AddParam(OAuthAuthRequestDefaults.RedirectUriKey, loginStartUri);
            AddParam(OAuthAuthRequestDefaults.ScopeKey, string.Join(" ", scopes));
            AddParam(OAuthAuthRequestDefaults.StateKey, _state);
            AddParam(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
            AddParam(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

            url.Query = queryParams.ToString();
            var configuredUri = url.Uri.ToString();
            if (InternalSettings.PrepareLoginStartUri is not null)
            {
                return await InternalSettings.PrepareLoginStartUri(ServiceProvider, Tokens, await Tokens.GetAsync(token), configuredUri, token);
            }
            return configuredUri;
        }
        return defaultReturn;
    }

    protected async virtual ValueTask<IDictionary<string, string>?> InternalExchangeCodeForTokensAsync(
        IDictionary<string, string> tokens,
        string authorizationCode,
        string? redirectUri,
        CancellationToken cancellationToken)
    {
        if (InternalSettings?.ClientOptions is not OAuthClientOptions clientOptions)
        {
            ProviderLogger.LogError("Client Options are not configured.");
            return default;
        }
        if (clientOptions.ClientID is not string clientID ||
            string.IsNullOrWhiteSpace(clientID))
        {
            ProviderLogger.LogError("Client ID is not configured.");
            return default;
        }
        if (string.IsNullOrWhiteSpace(_codeVerifier))
        {
            ProviderLogger.LogError("PKCE code verifier is missing.");
            return default;
        }
        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            ProviderLogger.LogError("Authorization code is missing.");
            return default;
        }
        if (clientOptions.CallbackUri is not string callbackUri
            || BeAValidUrl(callbackUri))
        {
            ProviderLogger.LogError("Callback URI is not configured.");
            return default;
        }
        try
        {
            var tokenResponse = await AuthEndpoints.ExchangeCodeAsync(new AccessTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.AuthorizationCode,
                ClientId = clientID,
                RedirectUri = callbackUri,
                Code = authorizationCode,
                CodeVerifier = _codeVerifier
            }, cancellationToken);
            if (tokenResponse is null)
            {
                ProviderLogger.LogError("Token response is null");
                return default;
            }

            if (tokenResponse.AccessToken is not string accessToken
                || tokenResponse.RefreshToken is not string refreshToken
                || tokenResponse.TokenType is not string tokenType || tokenType != "Bearer")
            {
                ProviderLogger.LogError("Token response missing required tokens");
                return default;
            }

            // Store tokens
            tokens[InternalSettings.TokenCacheOptions.AccessTokenKey] = accessToken;
            tokens[InternalSettings.TokenCacheOptions.RefreshTokenKey] = refreshToken;
            tokens[InternalSettings.TokenCacheOptions.ExpiresInKey] = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToString("g");

            if (InternalSettings.ExchangeCodeForTokensAsync is not null)
            {
                return await InternalSettings.ExchangeCodeForTokensAsync(ServiceProvider, Tokens, await Tokens.GetAsync(cancellationToken), redirectUri, cancellationToken);
            }
            return tokens;
        }
        catch (ApiException apiEx)
        {
            ProviderLogger.LogError(apiEx, "API error during code exchange: {StatusCode} - {ReasonPhrase} - {Content}", apiEx.StatusCode, apiEx.ReasonPhrase, apiEx.Content);

            return default;
        }
        catch (Exception ex)
        {
            ProviderLogger.LogError(ex, "Error exchanging authorization code for tokens");
            return default;
        }
    }
}