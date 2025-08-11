using DevTKSS.MyManufacturerERP.Infrastructure.Defaults;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

public sealed class EtsyOAuthAuthenticationService
{
    private const string ProviderName = "EtsyOAuth";

    private readonly IEtsyOAuthEndpoints _IAuthEndpoints;
    private readonly OAuthOptions _options; // Etsy "API Key string"
    private string? _state;
    private string? _codeVerifier;

    public EtsyOAuthAuthenticationService(
        IOptions<OAuthOptions> options,
        IEtsyOAuthEndpoints oAuthEndpoints)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        ArgumentOutOfRangeException.ThrowIfLessThan(_options.Scopes.Length,1, nameof(_options.Scopes));
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ClientID, nameof(_options.ClientID));
        _IAuthEndpoints = oAuthEndpoints;
    }

    public async Task<string> PrepareLoginStartUri(
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
        add(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.ResponseValueCode);
        add(OAuthAuthRequestDefaults.ClientIdKey, _options.ClientID);
        add(OAuthAuthRequestDefaults.RedirectUriKey, _options.RedirectUri);
        add(OAuthAuthRequestDefaults.ScopeKey, scope);
        add(OAuthAuthRequestDefaults.StateKey, _state);
        add(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
        add(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

        url.Query = sb.ToString();

        return url.Uri.ToString();
    }
    public async Task<string> PrepareLoginCallbackUri(
        IServiceProvider services,
        ITokenCache tokens,
        IDictionary<string, string>? credentials,
        string? loginCallbackUri,
        CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(loginCallbackUri) ? _options.RedirectUri! : loginCallbackUri;
    }
    public async Task<TokenResponse> ExchangeCodeAsync(
        IServiceProvider services,
        ITokenCache tokens,
        IDictionary<string, string>? queryParameters,
        string? accessGrantResponse, // This could have the query parameters from the callback URL or could be removed if the queryParameters parameter is containing them instead, but for error handling it is useful to have the full response
        string? tokenEndpoint, // Will not use this as I use refit client but others could need it when using HttpClient or similar
        CancellationToken ct)
    {
        string? state = null;
        string? code = null;
        queryParameters?.TryGetValue(OAuthAuthResponseDefaults.StateKey, out state);
        queryParameters?.TryGetValue(OAuthAuthResponseDefaults.CodeKey, out code);
        queryParameters?.TryGetValue(OAuthErrorResponseDefaults.ErrorKey, out var error);
        queryParameters?.TryGetValue(OAuthErrorResponseDefaults.ErrorDescriptionKey, out var errorDescription);
        queryParameters?.TryGetValue(OAuthErrorResponseDefaults.ErrorUriKey, out var errorUri);

        // Validate state and code
        if (string.IsNullOrWhiteSpace(state) || state != _state || string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException($"Invalid state or code. State: '{state}', Old State: '{_state}', Code: '{code}', Code Verifyer: {_codeVerifier}");
        }

        var tokenExchangeResult = await _IAuthEndpoints.ExchangeCodeAsync(
            grantType: OAuthTokenRefreshDefaults.GrantTypeValue,
            clientId: _options.ClientID!,
            redirectUri: _options.RedirectUri!,
            code: code,
            codeVerifier: _codeVerifier!
        );

        // Check for error in response
        if (string.IsNullOrEmpty(tokenExchangeResult.AccessToken) || string.IsNullOrEmpty(tokenExchangeResult.RefreshToken))
        {
            throw new InvalidOperationException($"Token exchange failed. Response missing access_token.");
        }

        await tokens.SaveTokensAsync(ProviderName, tokenExchangeResult.AccessToken,tokenExchangeResult.RefreshToken, ct);

        return tokenExchangeResult;
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

        var tokenResponse = await _IAuthEndpoints.RefreshTokenAsync(
            grantType: OAuthTokenRefreshDefaults.RefreshToken,
            clientId: _options.ClientID!,
            refreshToken: refreshToken
        );

        if (string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            throw new InvalidOperationException("Refresh response missing access_token or refresh_token.");

        await tokens.SaveTokensAsync(ProviderName, tokenResponse.AccessToken, tokenResponse.RefreshToken, ct);

        return tokenResponse;
    }
}