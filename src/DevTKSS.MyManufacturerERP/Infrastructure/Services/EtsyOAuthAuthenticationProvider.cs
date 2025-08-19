using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using Refit;
using DevTKSS.Extensions.OAuth;
using DevTKSS.Extensions.OAuth.Defaults;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

internal sealed partial record EtsyOAuthAuthenticationProvider 
{
    [GeneratedRegex(@"^(\\d+)\\.")]
    private static partial Regex DoesContainUserId();

    private const string ProviderName = "EtsyOAuth";

    private readonly IEtsyOAuthEndpoints _authEndpointsClient;
    private readonly IEtsyUserEndpoints _userEndpointsClient;
    private readonly ITasksManager _tasksManager;
    private readonly OAuthOptions _options; // Etsy "API Key string"
    private readonly ITokenCache _tokenCache;
    private string? _state;
    private string? _codeVerifier;

    public EtsyOAuthAuthenticationProvider(
        IOptions<OAuthOptions> options,
        IEtsyOAuthEndpoints oAuthEndpoints,
        IEtsyUserEndpoints userEndpoints,
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

    public async Task AuthenticateAsync(Uri preparedAuthorizationStartUri, CancellationToken cancellationToken = default)
    {
        string? error = null;
        string? errorDesc = null;
        try
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

            if (!authGrantResponse.IsSuccessStatusCode)
            {
                errorDesc = $"HTTP {authGrantResponse.StatusCode}";
                error = "Non-success status code";
                throw new InvalidOperationException($"OAuth authorization failed. Error: {error}, Description: {errorDesc}");
            }
        }
        catch (ApiException apiEx)
        {
            errorDesc = apiEx.Content;
            error = apiEx.StatusCode.ToString();
            // Try to parse as JSON for error_description
            if (!string.IsNullOrWhiteSpace(apiEx.Content))
            {
                try
                {
                    using var doc = JsonDocument.Parse(apiEx.Content);
                    if (doc.RootElement.TryGetProperty("error", out var errProp))
                    {
                        error = errProp.GetString();
                    }
                    if (doc.RootElement.TryGetProperty("error_description", out var descProp))
                    {
                        errorDesc = descProp.GetString();
                    }
                }
                catch
                {
                    // Fallback: treat as plain text
                    errorDesc = apiEx.Content;
                }
            }
            var message = $"OAuth authorization failed. Error: {error ?? "Unknown"}, Description: {errorDesc ?? "No description"}";
            Log.Error(message);
            throw new InvalidOperationException(message, apiEx);
        }
        catch (Exception ex)
        {
            var message = $"OAuth authorization failed. Error: {error ?? ex.GetType().Name}, Description: {errorDesc ?? ex.Message}";
            Log.Error(message);
            throw new InvalidOperationException(message, ex);
        }
    }
    internal async ValueTask<string> CreateLoginStartUri(
         IServiceProvider services,
         ITokenCache tokens,
         IDictionary<string, string>? credentials, // if this is null, can we use it for storing state and code verifier?
         string? loginStartUri,
         CancellationToken cancellationToken)
    {
        var options = services.GetRequiredService<IOptions<OAuthOptions>>().Value;
        credentials ??= new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(loginStartUri))
            loginStartUri = options.AuthorizationEndpoint!;

        var scope = string.Join(' ', options.Scopes);
        var state = OAuth2Utilitys.GenerateState();
        var codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier);

        var url = new UriBuilder(loginStartUri);
        var sb = new StringBuilder();
        void add(string k, string? v)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(k)).Append('=').Append(Uri.EscapeDataString(v ?? string.Empty));
        }
        add(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.CodeKey);
        add(OAuthAuthRequestDefaults.ClientIdKey, options.ClientID);
        add(OAuthAuthRequestDefaults.RedirectUriKey, options.RedirectUri);
        add(OAuthAuthRequestDefaults.ScopeKey, scope);
        add(OAuthAuthRequestDefaults.StateKey, state);
        add(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
        add(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

        url.Query = sb.ToString();

        credentials.AddOrReplace(OAuthAuthRequestDefaults.StateKey, state);
        credentials.AddOrReplace(OAuthPkceDefaults.CodeVerifierKey, codeVerifier);

        // in case the credentials is null better use backing fields
        _state = state;
        _codeVerifier = codeVerifier;

        return url.Uri.ToString();
    }

    // No idea how to instead integrate this to a seperate service but even then no idea how to open that damn browser
    private async ValueTask<IDictionary<string, string>?> ProcessPostLoginAsync(
        IEtsyOAuthEndpoints authEndpoints,
        IServiceProvider serviceProvider,
        ITokenCache tokenCache,
        IDictionary<string, string>? credentials,
        string redirectUri,
        IDictionary<string, string> tokens,
        CancellationToken ct = default)
    {
        var options = serviceProvider.GetRequiredService<IOptions<OAuthOptions>>().Value;
        if (credentials is null)
        {
            Log.Error("Credentials are null, cannot process post login. Will try to use backing fields.");
        }

        // Try to get state and codeVerifier from credentials, otherwise use backing fields
        string? state = null;
        string? codeVerifyer = null;
        if (credentials != null)
        {
            credentials.TryGetState(out state);
            credentials.TryGetCodeVerifier(out codeVerifyer);
        }
        if (string.IsNullOrEmpty(state))
            state = _state;
        if (string.IsNullOrEmpty(codeVerifyer))
            codeVerifyer = _codeVerifier;
        // TODO: Write code to process credentials that are passed into the LoginAsync method
        if (!string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(codeVerifyer))
        {
            // Copyed from Uno.Extensions.Web.WebAuthenticationProvider
            var query = redirectUri.StartsWith(options.RedirectUri!)
                ? AuthHttpUtility.ExtractArguments(redirectUri)  // authData is a fully qualified url, so need to extract query or fragment
                : AuthHttpUtility.ParseQueryString(redirectUri.TrimStart('#').TrimStart('?')); // authData isn't full url, so just process as query or fragment

            // The redirectUri does hold the full response of the AuthorizationBrokerProvider
            // so its including all eventual query parameters that are not covered by the AccessToken or RefreshToken keys
            // if https://github.com/unoplatform/uno.extensions/pull/2893 gets merged, you could also provide additional query keys via 'OtherTokenKeys' in the appsettings section for WebAuthenticationProvider 'WebConfiguration' normally aliased with 'Web'
            var returnedState = query?.Get(OAuthAuthResponseDefaults.StateKey); // why is the Get not working while the uno extensions is using it the same?
            var authorizationCode = query?.Get(OAuthAuthResponseDefaults.CodeKey);
            var error = query?.Get(OAuthErrorResponseDefaults.ErrorKey);
            var errorDescription = query?.Get(OAuthErrorResponseDefaults.ErrorDescriptionKey);
            var errorUri = query?.Get(OAuthErrorResponseDefaults.ErrorUriKey);

            // Validate state and code
            if (string.IsNullOrWhiteSpace(state) || returnedState != state || string.IsNullOrWhiteSpace(authorizationCode))
            {
                Log.Error("Invalid state or code. State: '{state}', Old State: '{oldState}', Code: '{authCode}', Code Verifyer: {codeVerifier}",
                    returnedState, state, authorizationCode, codeVerifyer);
                return default;
            }

            TokenResponse tokenExchangeResult = await authEndpoints.ExchangeCodeAsync(new AccessTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.AuthorizationCode,
                ClientId = options.ClientID!,
                RedirectUri = options.RedirectUri!,
                Code = authorizationCode,
                CodeVerifier = codeVerifyer!
            });

            if (!(string.IsNullOrWhiteSpace(tokenExchangeResult.AccessToken) || string.IsNullOrWhiteSpace(tokenExchangeResult.RefreshToken)))
            {
                Log.Error("Failed to exchange code for access token!");
                return default;
            }
            // extract userId from refreshToken you may need it later
            var match = DoesContainUserId().Match(tokenExchangeResult.AccessToken!);
            if (match.Success)
            {
                string userId = match.Groups[1].Value;
            }

            var expirationTimeStamp = DateTime.Now.AddSeconds(tokenExchangeResult.ExpiresIn).ToString("g");
            // Save additional tokens if needed
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.AccessTokenKey, tokenExchangeResult.AccessToken!);
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.RefreshToken, tokenExchangeResult.RefreshToken!);
            tokens.AddOrReplace(OAuthTokenRefreshExtendedDefaults.ExpirationDateKey, expirationTimeStamp);

            // remove the state and code verifier from credentials as they are no longer needed
            if (!credentials.TryRemoveKeys([OAuthAuthRequestDefaults.StateKey, OAuthPkceDefaults.CodeVerifierKey]))
            {
                Log.Warning("Failed to remove state and code verifier from credentials. Credentials was null? {Credentials}", credentials == null);
            }
            return tokens;
        }

        // Return null/default to fail the LoginAsync method
        return default;
    }

    private async ValueTask<IDictionary<string, string>?> RefreshTokensAsync(
        IEtsyOAuthEndpoints authEndpoints,
        IServiceProvider serviceProvider,
        ITokenCache tokenCache,
        IDictionary<string, string>? tokens,
        CancellationToken ct = default)
    {
        var options = serviceProvider.GetRequiredService<IOptions<OAuthOptions>>().Value;
        if (tokens is null)
        {
            Log.Error("Tokens are null, cannot refresh tokens.");
            return default;
        }

        // TODO: Write code to refresh tokens using the currently stored tokens
        if ((tokens?.TryGetRefreshToken(out var refreshToken) ?? false) && !refreshToken.IsNullOrWhiteSpace()
         && (tokens?.TryGetExpirationDate(out var tokenExpiry) ?? false) && tokenExpiry > DateTime.Now)
        {

            var tokenResponse = await authEndpoints.RefreshTokenAsync(new RefreshTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.RefreshToken,
                ClientId = options.ClientID!,
                RefreshToken = refreshToken!
            });

            if (string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                Log.Error("Refresh response missing access_token or refresh_token.");
                return default;
            }

            // Return IDictionary containing any tokens used by service calls or in the app
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.AccessTokenKey, tokenResponse.AccessToken);
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.RefreshToken, tokenResponse.RefreshToken);
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.ExpiresInKey, DateTime.Now.AddMinutes(5).ToString("g"));
            return tokens;
        }

        // Return null/default to fail the Refresh method
        return default;
    }
}