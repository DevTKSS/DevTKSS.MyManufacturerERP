// Import the namespace its defined (the one below) to make it available in the attribute which needs to be above the namespace declaration.
//[assembly:
//    ApiExtension(typeof(IWebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker))]

using DevTKSS.Extensions.OAuth.HttpListenerService;

namespace DevTKSS.Extensions.OAuth.AuthCallback;

public sealed class OAuthHttpListenerCallbackHandler : IHttpListenerCallbackHandler
{
    private readonly Uri _callbackUri;
    private readonly TaskCompletionSource<WebAuthenticationResult> _tcs = new();

    public OAuthHttpListenerCallbackHandler(Uri callbackUri, CancellationToken ct)
    {
        _callbackUri = callbackUri;
        ct.Register(() => _tcs.TrySetResult(new WebAuthenticationResult(null, 0, WebAuthenticationStatus.UserCancel)));
    }

    public async Task HandleRequest(HttpListenerCallback callback, string relativePath, CancellationToken ct)
    {
        if (callback.Request.Url is not null && callback.Request.Url.AbsolutePath.StartsWith(_callbackUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
        {
            var requestUriString = callback.Request.Url?.ToString() ?? string.Empty;
            // Checks for errors.
            var statusCode = GetStatusCode(callback.Request.QueryString);
            WebAuthenticationResult result = GetWebAuthenticationResult(statusCode, requestUriString);

            _tcs.TrySetResult(result);
            string responseMessage = GetAuthenticationResponseMessage(result);

            await callback.SetResponseAsync(
                responseMessage,
                System.Net.Mime.MediaTypeNames.Text.Plain,
                statusCode: (int)statusCode,
                cancellationToken: ct).ConfigureAwait(false);
        }
    }

    private static WebAuthenticationResult GetWebAuthenticationResult(uint statusCode, string requestUriString)
    {
        return statusCode switch
        {
            200 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.Success),
            403 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.UserCancel),
            _ => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.ErrorHttp),
        };
    }

    private static string GetAuthenticationResponseMessage(WebAuthenticationResult result)
    {
        return result.ResponseStatus switch
        {
            WebAuthenticationStatus.Success => "Authentication completed successfully - you can close this browser now.",
            WebAuthenticationStatus.UserCancel => "Authentication was cancelled by the user.",
            WebAuthenticationStatus.ErrorHttp => "Authentication failed due to an error.",
            _ => "Authentication completed - you can close this browser now."
        };
    }

    private static uint GetStatusCode(System.Collections.Specialized.NameValueCollection queryString)
    {
        if (queryString.Get(OAuthErrorResponseDefaults.ErrorKey) is string error)
        {
            return error switch
            {
                OAuthErrorResponseDefaults.AccessDenied => 403,
                OAuthErrorResponseDefaults.InvalidClient or OAuthErrorResponseDefaults.UnauthorizedClient or OAuthErrorResponseDefaults.InvalidScope => 401,
                OAuthErrorResponseDefaults.TemporarilyUnavailable => 503,
                OAuthErrorResponseDefaults.UnsupportedGrantType => 500,
                _ => 400 // For all others: Bad Request
            };

        }

        return 200;
    }

    public Task<WebAuthenticationResult> WaitForCallbackAsync()
    {
        return _tcs.Task;
    }

    public void Dispose() => _tcs.TrySetResult(new WebAuthenticationResult(null, 0, WebAuthenticationStatus.UserCancel));
}
