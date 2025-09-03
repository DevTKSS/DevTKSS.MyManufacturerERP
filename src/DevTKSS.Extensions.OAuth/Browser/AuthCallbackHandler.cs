// Import the namespace its defined (the one below) to make it available in the attribute which needs to be above the namespace declaration.
//[assembly:
//    ApiExtension(typeof(IWebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker))]

namespace DevTKSS.Extensions.OAuth.Browser;

public sealed class AuthCallbackHandler : IHttpListenerCallbackHandler
{
    private readonly Uri _callbackUri;
    private readonly TaskCompletionSource<WebAuthenticationResult> _tcs = new();
    private readonly ILogger<AuthCallbackHandler> _logger;

    public AuthCallbackHandler(Uri callbackUri, CancellationToken ct, ILogger<AuthCallbackHandler>? logger = null)
    {
        _callbackUri = callbackUri;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthCallbackHandler>.Instance;
        ct.Register(() => _tcs.TrySetResult(new WebAuthenticationResult(null, 0, WebAuthenticationStatus.UserCancel)));
    }

    public async Task HandleRequest(HttpListenerCallback callback, string relativePath, CancellationToken ct)
    {
        if (callback.Request.Url is not null && callback.Request.Url.AbsolutePath.StartsWith(_callbackUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
        {
            uint statusCode = 200;
            var requestUriString = callback.Request.Url?.ToString() ?? string.Empty;
            // Checks for errors.
            statusCode = GetStatusCode(callback.Request.QueryString);
            var result = statusCode switch
            {
                200 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.Success),
                403 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.UserCancel),
                _ => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.ErrorHttp),
            };

            _tcs.TrySetResult(result);
            await callback.SetResponseAsync(
                "Auth completed - you can close this browser now.",
                System.Net.Mime.MediaTypeNames.Text.Plain,
                statusCode: 200,
                cancellationToken: ct).ConfigureAwait(false);
        }
    }

    private uint GetStatusCode(System.Collections.Specialized.NameValueCollection queryString)
    {
        if (queryString.Get(OAuthErrorResponseDefaults.ErrorKey) is string error)
        {
            var errorDescription = queryString.Get(OAuthErrorResponseDefaults.ErrorDescriptionKey);
            // Intentionally not logging error_uri to avoid leaking potentially sensitive URLs
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("OAuth authorization error: '{Error}', error_description: '{Description}'", error, errorDescription);
            }

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
}
