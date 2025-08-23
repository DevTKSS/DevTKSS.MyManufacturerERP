

namespace DevTKSS.Extensions.OAuth.Browser;

public class HttpListenerService : IHttpListenerService
{
    private readonly ServerOptions _options;
    private readonly OAuthOptions _oAuthOptions;
    private readonly ILogger<HttpListenerService> _logger;
    private readonly IBrowserProvider _browserProvider;
    public HttpListenerService(
        ILogger<HttpListenerService> logger,
        IBrowserProvider browserProvider,
        IOptions<ServerOptions> serverOptions,
        IOptions<OAuthOptions> oAuthOptions
        )
    {
        _logger = logger;
        _browserProvider = browserProvider;
        _options = serverOptions.Value;
        _oAuthOptions = oAuthOptions.Value;
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_options.RootUri, nameof(_options.RootUri));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_options.CallbackUri, nameof(_options.CallbackUri));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_oAuthOptions.ClientID, nameof(_oAuthOptions.ClientID));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_oAuthOptions.AuthorizationEndpoint, nameof(_oAuthOptions.AuthorizationEndpoint));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_oAuthOptions.TokenEndpoint, nameof(_oAuthOptions.TokenEndpoint));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_oAuthOptions.UserInfoEndpoint, nameof(_oAuthOptions.UserInfoEndpoint));
        if (_oAuthOptions.Scopes == null || _oAuthOptions.Scopes.Length == 0)
            throw new ArgumentNullException(nameof(_oAuthOptions.Scopes), "At least one scope must be specified.");
    }

    public Uri GetCurrentApplicationCallbackUri()
    {
        var listener = new TcpListener(IPAddress.Loopback, _options.Port);
        listener.Start();
        var resultingPort = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return new Uri($"{_options.RootUri}:{resultingPort}/{_options.CallbackUri}");
    }

    public async Task<WebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri, Uri callbackUri, CancellationToken ct)
    {
        try
        {
            // Generates state and PKCE values.
            string state = OAuth2Utilitys.GenerateState();
            string codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
            string codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier);
            const string codeChallengeMethod = OAuthPkceDefaults.CodeChallengeMethodS256;

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = GetCurrentApplicationCallbackUri().AbsoluteUri;
            output("redirect URI: " + redirectURI);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            output("Listening..");
            http.Start();

            try
            {
                // Creates the OAuth 2.0 authorization request.
                string authorizationRequest = string.Format("{0}?response_type=code&scope={1}&redirect_uri={2}&client_id={3}&state={4}&code_challenge={5}&code_challenge_method={6}",
                    _oAuthOptions.AuthorizationEndpoint,
                    Uri.EscapeDataString(string.Join(' ', _oAuthOptions.Scopes)),
                    Uri.EscapeDataString(redirectURI),
                    _oAuthOptions.ClientID,
                    state,
                    codeChallenge,
                    codeChallengeMethod);

                // Opens request in the browser.
                _browserProvider.OpenBrowser(new Uri(authorizationRequest));

                // Waits for the OAuth authorization response.
                var context = await http.GetContextAsync();

                // Sends an HTTP response to the browser.
                var response = context.Response;
                string responseString = "<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                await responseOutput.WriteAsync(buffer, 0, buffer.Length);
                responseOutput.Close();

                // Checks for errors.
                if (context.Request.QueryString.Get(OAuthErrorResponseDefaults.ErrorKey) is string error)
                {
                    output($"OAuth authorization error: {error}");
                    return new WebAuthenticationResult(string.Empty, 400, WebAuthenticationStatus.ErrorHttp);
                }

                if (context.Request.QueryString.Get(OAuthAuthRequestDefaults.CodeKey) is not string code
                    || context.Request.QueryString.Get(OAuthAuthRequestDefaults.StateKey) is not string incomingState)
                {
                    output("Malformed authorization response. " + context.Request.QueryString);
                    return new WebAuthenticationResult(string.Empty, 400, WebAuthenticationStatus.ErrorHttp);
                }

                // Compares the received state to the expected value, to ensure that
                // this app made the request which resulted in authorization.
                if (incomingState != state)
                {
                    output($"Received request with invalid state ({incomingState})");
                    return new WebAuthenticationResult(string.Empty, 400, WebAuthenticationStatus.ErrorHttp);
                }

                output("Authorization code: " + code);

                // Call the PerformCodeExchangeAsync to exchange the code for tokens

                return await PerformCodeExchangeAsync(code, codeVerifier, redirectURI);
            }
            finally
            {
                http.Stop();
            }
        }
        catch (Exception ex)
        {
            output($"Authentication error: {ex.Message}");
            return new WebAuthenticationResult(string.Empty, 500, WebAuthenticationStatus.ErrorHttp);
        }
    }

    private async Task<WebAuthenticationResult> PerformCodeExchangeAsync(string code, string codeVerifier, string redirectURI)
    {
        output("Exchanging code for tokens...");

        string tokenRequestURI = _oAuthOptions.TokenEndpoint!;
        string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope={4}&grant_type=authorization_code",
            code,
            Uri.EscapeDataString(redirectURI),
            _oAuthOptions.ClientID,
            codeVerifier,
            Uri.EscapeDataString(string.Join(' ', _oAuthOptions.Scopes))
        );

        using var httpClient = new HttpClient();
        var content = new StringContent(tokenRequestBody, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        try
        {
            var tokenResponse = await httpClient.PostAsync(tokenRequestURI, content);
            var responseUri = tokenResponse.RequestMessage?.RequestUri?.ToString() ?? string.Empty;
            tokenResponse.EnsureSuccessStatusCode();

            string responseText = await tokenResponse.Content.ReadAsStringAsync();
            output(responseText);

            // Return the response URI as responseData
            return new WebAuthenticationResult(responseUri, 200, WebAuthenticationStatus.Success);
            
        }
        catch (HttpRequestException ex)
        {
            output($"HTTP error during token exchange: {ex.Message}");
            return new WebAuthenticationResult(string.Empty, 400, WebAuthenticationStatus.ErrorHttp);
        }
        catch (Exception ex)
        {
            output($"Error during token exchange: {ex.Message}");
            return new WebAuthenticationResult(string.Empty, 500, WebAuthenticationStatus.ErrorHttp);
        }
    }

    private void output(string output)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace($"[OAuth] {output}");
        }
    }
}
