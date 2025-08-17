namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

internal sealed class EtsyOAuthAuthenticationDelegate 
{
    private const string ProviderName = "EtsyOAuth";

    private readonly IEtsyOAuthEndpoints _authEndpointsClient;
    private readonly IEtsyUserEndpoints _userEndpointsClient;
    private readonly ITasksManager _tasksManager;
    private readonly OAuthOptions _options; // Etsy "API Key string"
    private readonly ITokenCache _tokenCache;
    private string? _state;
    private string? _codeVerifier;

    public EtsyOAuthAuthenticationDelegate(
        IOptions<OAuthOptions> options,
        IEtsyOAuthEndpoints oAuthEndpoints,
        IEtsyUserEndpoints userEndpoints,
        IHelpers oAuthHelpers,
        ITasksManager oAuthTasksManager,
        ITokenCache tokenCache)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        ArgumentOutOfRangeException.ThrowIfLessThan(_options.Scopes.Length, 1, nameof(_options.Scopes));
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ClientID, nameof(_options.ClientID));
        _userEndpointsClient = userEndpoints;
        _authEndpointsClient = oAuthEndpoints;
        _tokenCache = tokenCache;
    }

#region PrepareLoginStartUri and PrepareLoginCallbackUri for Uno.Extensions.Authentication.Web
    // This method prepares the login start URI for the OAuth flow and is used or should be used by the Uno.Extensions.Authentication.Web package
    // as it is unclear how to connect this to the Uno.Extensions.Authentication.Web WebAuthenticationProvider, I will have to replace it with own refit call instead.
    internal async Task<string> PrepareLoginStartUri(
        IServiceProvider services,
        ITokenCache tokens,
        IDictionary<string, string>? credentials,
        string? loginStartUri, // and here would be missing some kind of pkce boolean parameter missing, if we assume we not always have a seperate service like here that gets IOptions<OAuthOptions> injected already.
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(loginStartUri))
             loginStartUri = _options.AuthorizationEndpoint!;

        var scope = string.Join(' ', _options.Scopes);
        _state = OAuth2Utilitys.GenerateState();
        _codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(_codeVerifier);

        var url = new UriBuilder(loginStartUri);
        var sb = new StringBuilder();
        void add(string k, string? v)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(k)).Append('=').Append(Uri.EscapeDataString(v ?? string.Empty));
        }
        add(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.CodeKey);
        add(OAuthAuthRequestDefaults.ClientIdKey, _options.ClientID);
        add(OAuthAuthRequestDefaults.RedirectUriKey, _options.RedirectUri);
        add(OAuthAuthRequestDefaults.ScopeKey, scope);
        add(OAuthAuthRequestDefaults.StateKey, _state);
        add(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
        add(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

        url.Query = sb.ToString();

        return url.Uri.ToString();
    }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously 
    // This method is not awaiting anything as the loginCallbackUri does not hold any data at this point that could be extracted with the default keys Uno uses, which are not 'state' or 'code' then only 'access_token' and 'refresh_token'.
    // implement this if oAuth on desktop with AuthorizationCode flow is supported, otherwise this is not needed.
    public async Task<string> PrepareLoginCallbackUri(
        IServiceProvider services,
        ITokenCache tokens,
        IDictionary<string, string>? credentials,
        string? loginCallbackUri,
        CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(loginCallbackUri) ? _options.RedirectUri! : loginCallbackUri;
    }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously 
#endregion

    public async Task AuthenticateAsync(Uri preparedAuthorizationStartUri,CancellationToken cancellationToken = default)
    {

       var authGrantResponse = await _authEndpointsClient.SendAuthorizationCodeRequestAsync(new AuthorizationCodeRequest
       {
           ResponseType = OAuthAuthRequestDefaults.CodeKey,
           RedirectUri = _options.RedirectUri!,
           Scope = Uri.EscapeDataString(string.Join(' ', _options.Scopes)),
           ClientId = _options.ClientID!,
           State = _state!,
           CodeChallenge = _codeVerifier!,
           CodeChallengeMethod = OAuthPkceDefaults.CodeChallengeMethodS256
       });

        if(!authGrantResponse.IsSuccessStatusCode)
        {
            switch (authGrantResponse.Content!.Error)
            {
                case OAuthErrorResponseDefaults.InvalidRequest:
                    Log.Error("Invalid request: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
                case OAuthErrorResponseDefaults.InvalidClient:
                    Log.Error("Invalid client: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
                case OAuthErrorResponseDefaults.InvalidGrant:
                    Log.Error("Invalid grant: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
                case OAuthErrorResponseDefaults.UnauthorizedClient:
                    Log.Error("Unauthorized client: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
                case OAuthErrorResponseDefaults.UnsupportedGrantType:
                    Log.Error("Unsupported grant type: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
                case OAuthErrorResponseDefaults.AccessDenied:
                    Log.Error("Access denied: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
                case OAuthErrorResponseDefaults.InvalidScope:
                    Log.Error("Invalid scope: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
                default:
                    Log.Error("Unknown error: {ErrorDescription}", authGrantResponse.Content.ErrorDescription);
                    break;
            }
        }
    }
    // can be called by the PostLogin Delegate from the signature, but how to connect this service method to it?
    // This method is called after the user has authenticated and the authorization code has been received.
    public async Task<bool> ExchangeTokenAsync(
        IEtsyOAuthEndpoints authEndpoints,
        IServiceProvider serviceProvider,
        ITokenCache tokenCache,
        IDictionary<string, string> credentials,
        string redirectUri,
        IDictionary<string, string> tokens,
        CancellationToken ct)
    {
        string? state = null;
        string? authCode = null;
        // Assuming the queryParameters would be holding the query parameters from the callback URL
        credentials?.TryGetValue(OAuthAuthResponseDefaults.StateKey, out state);
        credentials?.TryGetValue(OAuthAuthResponseDefaults.CodeKey, out authCode);
        credentials?.TryGetValue(OAuthErrorResponseDefaults.ErrorKey, out var error);
        credentials?.TryGetValue(OAuthErrorResponseDefaults.ErrorDescriptionKey, out var errorDescription);
        credentials?.TryGetValue(OAuthErrorResponseDefaults.ErrorUriKey, out var errorUri);

        // Validate state and code
        if (string.IsNullOrWhiteSpace(state) || state != _state || string.IsNullOrWhiteSpace(authCode))
        {
            Log.Error("Invalid state or code. State: '{state}', Old State: '{oldState}', Code: '{authCode}', Code Verifyer: {codeVerifier}",
                state, _state, authCode, _codeVerifier);
            return false;
        }
        
        var tokenExchangeResult = await authEndpoints.ExchangeCodeAsync(new AccessTokenRequest
        {
            GrantType = OAuthTokenRefreshDefaults.AuthorizationCode,
            ClientId = _options.ClientID!,
            RedirectUri = _options.RedirectUri!,
            Code = authCode,
            CodeVerifier = _codeVerifier!
        });

        await _tokenCache.SaveTokensAsync(ProviderName, tokenExchangeResult.AccessToken, tokenExchangeResult.RefreshToken, ct);

        // Save additional tokens if needed
        var additionalTokensDict = new Dictionary<string, string>();
        additionalTokensDict.TryAdd(OAuthTokenRefreshDefaults.ExpiresInKey, tokenExchangeResult.ExpiresIn.ToString());

        await _tokenCache.SaveAsync(ProviderName, additionalTokensDict, ct);

        return true;
    }

    public async Task<TokenResponse> RefreshAsync(
        IServiceProvider services,
        ITokenCache tokens,
        IDictionary<string, string> credentials,
        IDictionary<string, string>? extraParams,
        CancellationToken ct = default)
    {
        // Extract refresh token from credentials
        if (!credentials.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            throw new InvalidOperationException("Missing refresh_token in credentials.");

        var tokenResponse = await _authEndpointsClient.RefreshTokenAsync(new RefreshTokenRequest
        {
            GrantType = OAuthTokenRefreshDefaults.RefreshToken,
            ClientId = _options.ClientID!,
            RefreshToken = refreshToken
        });

        if (string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            throw new InvalidOperationException("Refresh response missing access_token or refresh_token.");

        await tokens.SaveTokensAsync(ProviderName, tokenResponse.AccessToken, tokenResponse.RefreshToken, ct);

        return tokenResponse;
    }
}