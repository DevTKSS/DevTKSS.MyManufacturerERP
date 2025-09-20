using System.Collections.Specialized;
using System.Net;
using DevTKSS.Extensions.OAuth.Endpoints;
using Microsoft.Extensions.Configuration;
using static DevTKSS.Extensions.OAuth.Validation.UriValidationUtility;

namespace DevTKSS.Extensions.OAuth.OAuthServices;
/// <summary>
/// Service for handling OAuth authentication flows (desktop loopback via system browser).
/// </summary>
public record OAuthProvider(
        ILogger<OAuthProvider> ProviderLogger,
        IServiceProvider ServiceProvider,
        IOAuthEndpoints AuthEndpoints,
        ITokenCache Tokens,
        IOptionsSnapshot<OAuthOptions> Configuration,
        ISystemBrowserAuthBrokerProvider AuthBrowserProvider) 
    : BaseAuthenticationProvider(ProviderLogger,DefaultName,Tokens)
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
    private async ValueTask<(string? redirectUri, Dictionary<string,string> tokens, NameValueCollection? queryParams)> GetAuthenticationDataAsync(CancellationToken cancellationToken)
    {
        var loginStartUri = InternalSettings.LoginStartUri;
        loginStartUri = await InternalPrepareLoginStartUri(loginStartUri, cancellationToken);

        if (loginStartUri is null ||
            string.IsNullOrWhiteSpace(loginStartUri))
        {
            if (ProviderLogger.IsEnabled(LogLevel.Warning))
            {
                ProviderLogger.LogWarning($"{nameof(InternalSettings.LoginStartUri)} not specified, unable to start login flow");
            }
            return default;
        }

        var loginCallbackUri = InternalSettings.ClientOptions?.CallbackUri;

        if (string.IsNullOrWhiteSpace(loginCallbackUri) &&
            loginStartUri.Contains(OAuthAuthRequestDefaults.RedirectUriKey))
        {
            var args = AuthHttpUtility.ExtractArguments(loginStartUri);
            loginCallbackUri = args[OAuthAuthRequestDefaults.RedirectUriKey];
        }

        if (string.IsNullOrWhiteSpace(loginCallbackUri))
        {
            if (ProviderLogger.IsEnabled(LogLevel.Warning))
            {
                ProviderLogger.LogWarning($"{nameof(InternalSettings.ClientOptions.CallbackUri)} not specified and {OAuthAuthRequestDefaults.RedirectUriKey} not set in {nameof(InternalSettings.LoginStartUri)}, unable to start login flow");
            }
            return default;
        }

#if __IOS__
		WinRTFeatureConfiguration.WebAuthenticationBroker.PrefersEphemeralWebBrowserSession = InternalSettings.PrefersEphemeralWebBrowserSession;
#endif

#if WINDOWS
		var userResult = await WinUIEx.WebAuthenticator.AuthenticateAsync(new Uri(loginStartUri), new Uri(loginCallbackUri));
		var authData = string.Join("&", userResult.Properties.Select(x => $"{x.Key}={x.Value}"))??string.Empty;
#else
        var SystemBrowser = ServiceProvider.GetRequiredService<ISystemBrowserAuthBrokerProvider>();
        var userResult = await SystemBrowser.AuthenticateAsync(WebAuthenticationOptions.None, new Uri(loginStartUri), new Uri(loginCallbackUri), cancellationToken);
        var authData = userResult?.ResponseData ?? string.Empty;

#endif
        var query = authData.StartsWith(loginCallbackUri) ?
            AuthHttpUtility.ExtractArguments(authData) : // authData is a fully qualified url, so need to extract query or fragment
            AuthHttpUtility.ParseQueryString(authData.TrimStart('#').TrimStart('?')); // authData isn't full url, so just process as query or fragment

        var tokens = new Dictionary<string, string>();
        if (query is null)
        {
            return (null, tokens,null);
        }

        var accessToken = query.Get(InternalSettings.UriTokenOptions.AccessTokenKey);
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            tokens.AddOrReplace(InternalSettings.TokenCacheOptions.AccessTokenKey, accessToken);
        }

        var refreshToken = query.Get(InternalSettings.UriTokenOptions.RefreshTokenKey);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            tokens.AddOrReplace(InternalSettings.UriTokenOptions.RefreshTokenKey, refreshToken);
        }

        var idToken = query.Get(InternalSettings.UriTokenOptions.IdTokenKey);
        if (!string.IsNullOrWhiteSpace(idToken))
        {
            tokens.AddOrReplace(InternalSettings.TokenCacheOptions.IdTokenKey, idToken);
        }

        foreach (var (tokenCacheKey, uriKey) in InternalSettings.UriTokenOptions.OtherTokenKeys)
        {
            var uriValue = query.Get(uriKey);
            if (!string.IsNullOrWhiteSpace(uriValue))
            {
                tokens.AddOrReplace(tokenCacheKey, uriValue);
            }
        }

        return (authData, tokens, query);
    }
    protected async override ValueTask<IDictionary<string, string>?> InternalLoginAsync(
        IDispatcher? dispatcher,
        IDictionary<string, string>? credentials,
        CancellationToken cancellationToken)
    {

        (string? redirectUri,Dictionary<string,string> tokens, NameValueCollection? queryParams) = await GetAuthenticationDataAsync(cancellationToken);

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
    protected async virtual Task<string?> PrepareLoginCallbackUri(string? loginCallbackUri, CancellationToken cancellationToken)
    {
        if (InternalSettings.PrepareLoginCallbackUri is not null)
        {
            return await InternalSettings.PrepareLoginCallbackUri(ServiceProvider, Tokens, loginCallbackUri, cancellationToken);
        }
        return loginCallbackUri;
    }
    protected async virtual ValueTask<IDictionary<string, string>?> PostLogin(string redirectUri, IDictionary<string, string> tokens, CancellationToken cancellationToken)
    {
        if (InternalSettings.PostLoginCallback is not null)
        {
            return await InternalSettings.PostLoginCallback(ServiceProvider, Tokens, redirectUri, tokens, cancellationToken);
        }
        return tokens;
    }
    protected async override ValueTask<IDictionary<string, string>?> InternalRefreshAsync(
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

    protected async virtual ValueTask<string?> InternalPrepareLoginStartUri(
        string? loginStartUri,
        CancellationToken token)
    {

        if (InternalSettings is null)
        {
            ProviderLogger.LogError("OAuth Settings are not configured.");
            return default;
        }

        if (string.IsNullOrWhiteSpace(loginStartUri))
        {
            return default;
        }
        if (InternalSettings.ClientOptions is not OAuthClientOptions clientOptions)
        {
            ProviderLogger.LogError("{ClientOptions}: Client options are not configured.", nameof(OAuthClientOptions));
            return default;
        }
        if (clientOptions.EndpointOptions?.AuthorizationEndpoint is not string authEndpoint
            || BeAValidUrl(authEndpoint))
        {
            ProviderLogger.LogError("{AuthorizationEndpoint}: Authorization endpoint is not configured.", nameof(OAuthEndpointOptions.AuthorizationEndpoint));
            return default;
        }
        if (clientOptions.ClientID is not string clientID ||
            string.IsNullOrWhiteSpace(clientID))
        {
            ProviderLogger.LogError("{ClientID}: Client ID is not valid configured.", nameof(OAuthClientOptions.ClientID));
            return default;
        }
        if (clientOptions.Scopes is not { } scopes
            || scopes.Length == 0)
        {
            ProviderLogger.LogError("{Scopes}: At least one scope must be configured and have at least one entry.", nameof(OAuthClientOptions.Scopes));
            return default;
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
                return await PrepareLoginStartUri(configuredUri, token);
            }
            return configuredUri;
        }
        return default;
    }
    protected async virtual Task<string?> PrepareLoginStartUri(string? loginStartUri, CancellationToken cancellationToken)
    {
        if (InternalSettings.PrepareLoginStartUri is not null)
        {
            return await InternalSettings.PrepareLoginStartUri(ServiceProvider, Tokens, loginStartUri, cancellationToken);
        }
        return loginStartUri;
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

            if (InternalSettings.CodeExchangeCallback is not null)
            {
                return await InternalSettings.CodeExchangeCallback(ServiceProvider, Tokens, await Tokens.GetAsync(cancellationToken), redirectUri, cancellationToken);
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
public record OAuthProvider<TService>(
        ILogger<OAuthProvider<TService>> ServiceLogger,
        IServiceProvider ServiceProvider,
        IOAuthEndpoints AuthEndpoints,
        ITokenCache Tokens,
        IOptionsSnapshot<OAuthOptions> Configuration,
        ISystemBrowserAuthBrokerProvider AuthBrowserProvider) 
    : OAuthProvider(ServiceLogger, ServiceProvider, AuthEndpoints, Tokens, Configuration, AuthBrowserProvider)
    where TService : notnull
{

    public OAuthSettings<TService>? TypedSettings
    {
        get => base.Settings as OAuthSettings<TService>;
        init => base.Settings = value;
    }
    protected async override Task<string?> PrepareLoginStartUri(string? loginStartUri, CancellationToken cancellationToken)
    {
        if (TypedSettings?.PrepareLoginStartUri is not null)
        {
            return await TypedSettings.PrepareLoginStartUri(ServiceProvider.GetRequiredService<TService>(), ServiceProvider, Tokens, loginStartUri, cancellationToken);
        }
        return await base.PrepareLoginStartUri(loginStartUri, cancellationToken);
    }

    protected async override Task<string?> PrepareLoginCallbackUri(string? loginCallbackUri, CancellationToken cancellationToken)
    {
        if (TypedSettings?.PrepareLoginCallbackUri is not null)
        {
            return await TypedSettings.PrepareLoginCallbackUri(ServiceProvider.GetRequiredService<TService>(), ServiceProvider, Tokens, loginCallbackUri, cancellationToken);
        }
        return await base.PrepareLoginCallbackUri(loginCallbackUri, cancellationToken);
    }
    protected async override ValueTask<IDictionary<string, string>?> PostLogin(string redirectUri, IDictionary<string, string> tokens, CancellationToken cancellationToken)
    {
        if (TypedSettings?.PostLoginCallback is not null)
        {
            return await TypedSettings.PostLoginCallback(ServiceProvider.GetRequiredService<TService>(), ServiceProvider, Tokens, redirectUri, tokens, cancellationToken);
        }
        return await base.PostLogin(redirectUri, tokens, cancellationToken);
    }
    protected async override ValueTask<IDictionary<string, string>?> InternalRefreshAsync(CancellationToken cancellationToken)
    {

        if (TypedSettings?.RefreshCallback is not null)
        {
            return await TypedSettings.RefreshCallback(ServiceProvider.GetRequiredService<TService>(), ServiceProvider, Tokens, await Tokens.GetAsync(cancellationToken), cancellationToken);
        }
        return await base.InternalRefreshAsync(cancellationToken);
    }

    protected async override ValueTask<IDictionary<string, string>?> InternalExchangeCodeForTokensAsync(IDictionary<string, string> tokens, string authorizationCode, string? redirectUri, CancellationToken cancellationToken)
    {
        if(TypedSettings?.CodeExchangeCallback is not null)
        {
            return await TypedSettings.CodeExchangeCallback(ServiceProvider.GetRequiredService<TService>(), ServiceProvider, Tokens, await Tokens.GetAsync(cancellationToken), redirectUri, cancellationToken);
        }
        return await base.InternalExchangeCodeForTokensAsync(tokens, authorizationCode, redirectUri, cancellationToken);
    }

}