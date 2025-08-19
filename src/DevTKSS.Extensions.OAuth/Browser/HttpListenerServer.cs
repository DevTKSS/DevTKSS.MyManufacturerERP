using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DevTKSS.Extensions.OAuth.Defaults;
using DevTKSS.MyManufacturerERP.Models;
using Windows.Security.Authentication.Web;

namespace Temp.Extensibility.DesktopAuthBroker;

public class HttpListenerServer
{
    private readonly ServerOptions _options;
    private readonly OAuthOptions _oAuthOptions;

    public HttpListenerServer(
        IOptions<ServerOptions> serverOptions,
        IOptions<OAuthOptions> oAuthOptions)
    {
        _options = serverOptions.Value;
        _oAuthOptions = oAuthOptions.Value;
    }

    public Uri GetCurrentApplicationCallbackUri()
    {
        var listener = new TcpListener(IPAddress.Loopback, _options.Port);
        listener.Start();
        var resultingPort = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return new Uri($"{_options.RootUri}:{resultingPort}{_options.RelativeCallbackUri}");
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
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = authorizationRequest,
                    UseShellExecute = true
                });

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
                if (context.Request.QueryString.Get("error") != null)
                {
                    var error = context.Request.QueryString.Get("error");
                    output($"OAuth authorization error: {error}");
                    return new WebAuthenticationResult(string.Empty, 400, WebAuthenticationStatus.ErrorHttp);
                }

                if (context.Request.QueryString.Get(OAuthAuthRequestDefaults.CodeKey) == null
                    || context.Request.QueryString.Get(OAuthAuthRequestDefaults.StateKey) == null)
                {
                    output("Malformed authorization response. " + context.Request.QueryString);
                    return new WebAuthenticationResult(string.Empty, 400, WebAuthenticationStatus.ErrorHttp);
                }

                // extracts the code
                var code = context.Request.QueryString.Get(OAuthAuthRequestDefaults.CodeKey);
                var incoming_state = context.Request.QueryString.Get(OAuthAuthRequestDefaults.StateKey);

                // Compares the received state to the expected value, to ensure that
                // this app made the request which resulted in authorization.
                if (incoming_state != state)
                {
                    output($"Received request with invalid state ({incoming_state})");
                    return new WebAuthenticationResult(string.Empty, 400, WebAuthenticationStatus.ErrorHttp);
                }

                output("Authorization code: " + code);

                // Return successful result with the callback URL containing the code
                var callbackUrl = $"{redirectURI}?code={code}&state={state}";
                return new WebAuthenticationResult(callbackUrl, 200, WebAuthenticationStatus.Success);
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

    async void PerformCodeExchange(string code, string codeVerifier, string redirectURI)
    {
        output("Exchanging code for tokens...");

        // builds the request
        string tokenRequestURI = _oAuthOptions.TokenEndpoint ?? "https://openapi.etsy.com/v3/public/oauth/token";
        string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope={4}&grant_type=authorization_code",
            code,
            Uri.EscapeDataString(redirectURI),
            _oAuthOptions.ClientID,
            codeVerifier,
            Uri.EscapeDataString(string.Join(' ', _oAuthOptions.Scopes))
            );

        // sends the request
        using var httpClient = new HttpClient();
        var content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

        try
        {
            // gets the response
            var tokenResponse = await httpClient.PostAsync(tokenRequestURI, content);
            tokenResponse.EnsureSuccessStatusCode();

            // reads response body
            string responseText = await tokenResponse.Content.ReadAsStringAsync();
            output(responseText);

            // converts to dictionary
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var tokenEndpointDecoded = JsonSerializer.Deserialize<Dictionary<string, object>>(responseText, options);

            if (tokenEndpointDecoded != null && tokenEndpointDecoded.TryGetValue("access_token", out var accessTokenObj))
            {
                string access_token = accessTokenObj.ToString() ?? string.Empty;
                UserInfoCallAsync(access_token);
            }
        }
        catch (HttpRequestException ex)
        {
            output($"HTTP error during token exchange: {ex.Message}");
        }
        catch (Exception ex)
        {
            output($"Error during token exchange: {ex.Message}");
        }
    }

    public async void UserInfoCallAsync(string access_token, CancellationToken ct = default)
    {
        output("Making API Call to Userinfo...");

        // builds the request
        string userinfoRequestURI = _oAuthOptions.UserInfoEndpoint ?? "https://openapi.etsy.com/v3/application/users/me";

        // sends the request
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);

        try
        {
            // gets the response
            HttpResponseMessage userinfoResponse = await httpClient.GetAsync(userinfoRequestURI, ct);
            userinfoResponse.EnsureSuccessStatusCode();

            // reads response body
            string userinfoResponseText = await userinfoResponse.Content.ReadAsStringAsync(ct);
            output(userinfoResponseText);
        }
        catch (HttpRequestException ex)
        {
            output($"Error making userinfo request: {ex.Message}");
        }
    }

    /// <summary>
    /// Appends the given string to the on-screen log, and the debug console.
    /// </summary>
    /// <param name="output">string to be appended</param>
    public void output(string output)
    {
        // Use Console.WriteLine instead of textBoxOutput which doesn't exist
        Console.WriteLine($"[OAuth] {output}");
    }
}
