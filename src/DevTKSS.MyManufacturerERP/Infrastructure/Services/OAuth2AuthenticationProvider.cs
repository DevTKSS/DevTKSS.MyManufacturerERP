using System.Net;
using System.Text;
using System.Web;
using DevTKSS.MyManufacturerERP.Infrastructure.Entitys;
using Microsoft.Extensions.Options;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

public record OAuth2AuthenticationProvider(
    ILogger<OAuth2AuthenticationProvider> ProviderLogger,
    IOptionsMonitor<OAuthConfiguration> Configuration,
    IServiceProvider Services,
    ITokenCache Tokens
) : BaseAuthenticationProvider(ProviderLogger, DefaultName, Tokens)
{
    private const string OAuthRedirectUriParameter = "redirect_uri";
    public const string DefaultName = "Etsy";

    private OAuthConfiguration InternalSettings => Configuration.Get(DefaultName);

    private UserMe? _currentUser;
    private string? _lastState;
    private string CodeVerifier { get; set; } = string.Empty;
    private string CodeChallenge { get; set; } = string.Empty;
    private TokenResponse? _currentToken;
    private string? _refreshToken;
    private DateTimeOffset? _tokenExpiry;

    /// <summary>
    /// Initiates the OAuth2 login flow for Etsy and returns the resulting tokens and user information.
    /// </summary>
    /// <param name="dispatcher">Dispatcher for UI/browser actions.</param>
    /// <param name="credentials">Optional credentials dictionary (not used for Etsy).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary with access_token, refresh_token, expires_in, and user_id.</returns>
    protected async override ValueTask<IDictionary<string, string>?> InternalLoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials, CancellationToken cancellationToken)
    {
        var success = await LoginWithOAuthFlowAsync(dispatcher, cancellationToken);
        if (!success)
            return null;
    var tokenDict = new Dictionary<string, string>
    {
        { "access_token", _currentToken?.AccesToken ?? "" },
        { "refresh_token", _currentToken?.RefreshToken ?? "" },
        { "expires_in", _currentToken?.ExpiresIn.ToString() ?? "0" },
        { "user_id", _currentUser?.UserId.ToString() ?? "" }
    };
    await Tokens.SaveAsync(DefaultName, tokenDict, cancellationToken);

    return tokenDict;
    }

    /// <summary>
    /// Refreshes the access token using the stored refresh token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary with refreshed access_token, refresh_token, and expires_in.</returns>
    protected async override ValueTask<IDictionary<string, string>?> InternalRefreshAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_refreshToken))
            return null;
        using var client = new HttpClient();
        var payload = new
        {
            grant_type = "refresh_token",
            client_id = InternalSettings.ApiKey,
            refresh_token = _refreshToken
        };
        var json = JsonSerializer.Serialize(payload, EtsyJsonContext.Default.TokenRequestPayload);
        var content = new StringContent(json, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
        var response = await client.PostAsync(InternalSettings.TokenEndpoint, content, cancellationToken).ConfigureAwait(false);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogError("Failed to refresh Etsy OAuth token. Status: {StatusCode}, Response: {Response}", response.StatusCode, responseJson);
            return null;
        }
        var token = JsonSerializer.Deserialize<TokenResponse>(responseJson, EtsyJsonContext.Default.TokenResponse);
        if (token == null)
        {
            Logger.LogError(_lastState != null ? "Failed to deserialize token response for state {State}." : "Failed to deserialize token response.", _lastState);
            return null;
        }
        OAuth2Utilitys.SetTokenState(ref _currentToken, ref _refreshToken, ref _tokenExpiry, token);
        return new Dictionary<string, string>
        {
            { "access_token", _currentToken?.AccesToken ?? "" },
            { "refresh_token", _currentToken?.RefreshToken ?? "" },
            { "expires_in", _currentToken?.ExpiresIn.ToString() ?? "0" }
        };
    }

    /// <summary>
    /// Logs out the current user by clearing all token and user information.
    /// </summary>
    /// <param name="dispatcher">Dispatcher for UI/browser actions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if logout was successful.</returns>
    protected async override ValueTask<bool> InternalLogoutAsync(IDispatcher? dispatcher, CancellationToken cancellationToken)
    {
        _currentToken = null;
        _currentUser = null;
        _refreshToken = null;
        _tokenExpiry = null;
        return true;
    }

    // --- Etsy/PKCE spezifische Hilfsmethoden ---
    /// <summary>
    /// Starts the OAuth2 login flow, opens the browser, and handles the callback.
    /// </summary>
    /// <param name="dispatcher">Dispatcher for UI/browser actions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if login was successful, otherwise false.</returns>
    private async Task<bool> LoginWithOAuthFlowAsync(IDispatcher? dispatcher, CancellationToken cancellationToken)
    {
        var redirectUri = InternalSettings.RedirectUri;
        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri.EndsWith("/") ? redirectUri : redirectUri + "/");
        listener.Start();
        var authUrl = GetAuthorizationUrl();
        await dispatcher.ExecuteAsync(() =>
        {
            try
            {
#if WINDOWS || WINDOWS_UWP || HAS_WINUI
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });
#elif ANDROID
                var intent = new Android.Content.Intent(Android.Content.Intent.ActionView, Android.Net.Uri.Parse(authUrl));
                intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
#elif IOS
                Foundation.NSUrl url = new Foundation.NSUrl(authUrl);
                UIKit.UIApplication.SharedApplication.OpenUrl(url);
#elif WASM
                _ = Uno.Foundation.WebAssemblyRuntime.InvokeAsync($"window.open('{authUrl}', '_blank')");
#else
                System.Diagnostics.Process.Start(authUrl);
#endif
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fehler beim Öffnen des Browsers für OAuth Login.");
            }
        });
        var contextTask = listener.GetContextAsync();
        if (await Task.WhenAny(contextTask, Task.Delay(TimeSpan.FromMinutes(2), cancellationToken)) != contextTask)
        {
            Logger.LogWarning("OAuth2 callback timed out after 2 minutes.");
            return false;
        }
        var context = contextTask.Result;
        var query = context.Request.QueryString;
        string? code = query["code"];
        string? state = query["state"];
        context.Response.StatusCode = 200;
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Sie können dieses Fenster schließen!"));
        context.Response.Close();
        listener.Stop();
        if (!VerifyState(state))
            return false;
        var (token, user) = await AuthenticateAndFetchUserAsync(code!);
        _currentToken = token;
        _currentUser = user;
        OAuth2Utilitys.SetTokenState(ref _currentToken, ref _refreshToken, ref _tokenExpiry, token);
        return true;
    }

    /// <summary>
    /// Generates the Etsy OAuth2 authorization URL with PKCE parameters.
    /// </summary>
    /// <returns>The authorization URL for the OAuth2 flow.</returns>
    private string GetAuthorizationUrl()
    {
        _lastState = OAuth2Utilitys.GenerateState();
        CodeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        CodeChallenge = OAuth2Utilitys.GenerateCodeChallenge(CodeVerifier);
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["response_type"] = "code";
        query["client_id"] = InternalSettings.ApiKey;
        query["redirect_uri"] = InternalSettings.RedirectUri;
        query["scope"] = string.Join(" ", InternalSettings.Scopes);
        query["state"] = _lastState;
        query["code_challenge"] = CodeChallenge;
        query["code_challenge_method"] = "S256";
        return $"{InternalSettings.AuthorizationEndpoint}?{query}";
    }

    /// <summary>
    /// Exchanges the authorization code for tokens and fetches the current user information from Etsy.
    /// </summary>
    /// <param name="code">The authorization code received from the OAuth2 callback.</param>
    /// <returns>A tuple containing the token response and user information.</returns>
    private async Task<(TokenResponse token, UserMe user)> AuthenticateAndFetchUserAsync(string code)
    {
        var token = await ExchangeCodeForTokenAsync(code);
        OAuth2Utilitys.SetTokenState(ref _currentToken, ref _refreshToken, ref _tokenExpiry, token);
        var etsyApi = RestService.For<IEtsyUserEndpoints>("https://openapi.etsy.com");
        var bearerToken = $"Bearer {token.AccesToken}";
        var user = await etsyApi.GetMeAsync(bearerToken, InternalSettings.ApiKey);
        return (token, user);
    }

    /// <summary>
    /// Exchanges the authorization code for access and refresh tokens using PKCE.
    /// </summary>
    /// <param name="code">The authorization code received from the OAuth2 callback.</param>
    /// <returns>The token response containing access and refresh tokens.</returns>
    private async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
    {
        using var client = new HttpClient();
        var payload = new TokenRequestPayload
        {
            client_id = InternalSettings.ApiKey,
            redirect_uri = InternalSettings.RedirectUri,
            code = code,
            code_verifier = CodeVerifier
        };
        var json = JsonSerializer.Serialize(payload, EtsyJsonContext.Default.TokenRequestPayload);
        var content = new StringContent(json, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
        var response = await client.PostAsync(InternalSettings.TokenEndpoint, content).ConfigureAwait(false);
        var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Token exchange failed: {response.StatusCode} {responseJson}");
        }
        var token = JsonSerializer.Deserialize<TokenResponse>(responseJson, EtsyJsonContext.Default.TokenResponse);
        if (token == null)
            throw new InvalidOperationException("Failed to deserialize token response.");
        OAuth2Utilitys.SetTokenState(ref _currentToken, ref _refreshToken, ref _tokenExpiry, token);
        return token;
    }

    /// <summary>
    /// Verifies the state parameter from the OAuth2 callback to prevent CSRF attacks.
    /// </summary>
    /// <param name="stateFromCallback">The state value received from the callback.</param>
    /// <returns>True if the state matches the expected value; otherwise, false.</returns>
    private bool VerifyState(string stateFromCallback) => _lastState != null && _lastState == stateFromCallback;

    /// <summary>
    /// Initiates the login process and returns the resulting tokens and user information.
    /// </summary>
    /// <param name="dispatcher">Dispatcher for UI/browser actions.</param>
    /// <param name="credentials">Optional credentials dictionary (not used for Etsy).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary with access_token, refresh_token, expires_in, and user_id.</returns>
    public async ValueTask<IDictionary<string, string>?> LoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials, CancellationToken cancellationToken)
        => await InternalLoginAsync(dispatcher, credentials, cancellationToken);

    /// <summary>
    /// Logs out the current user by clearing all token and user information.
    /// </summary>
    /// <param name="dispatcher">Dispatcher for UI/browser actions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if logout was successful.</returns>
    public async ValueTask<bool> LogoutAsync(IDispatcher? dispatcher, CancellationToken cancellationToken)
        => await InternalLogoutAsync(dispatcher, cancellationToken);

    /// <summary>
    /// Refreshes the access token using the stored refresh token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary with refreshed access_token, refresh_token, and expires_in.</returns>
    public async ValueTask<IDictionary<string, string>?> RefreshAsync(CancellationToken cancellationToken)
        => await InternalRefreshAsync(cancellationToken);
}


