using System.Collections.Specialized;

namespace DevTKSS.Extensions.OAuth.AuthCallback;
public interface IAuthCallbackHandler : IHttpHandler
{
    public Uri CallbackUri { get; }
    public Task<WebAuthenticationResult> WaitForCallbackAsync();
}
public record AuthCallbackHandler : IAuthCallbackHandler
{    
    private readonly TaskCompletionSource<WebAuthenticationResult> _tcs = new();
    public Uri CallbackUri { get; init; }
    public AuthCallbackHandler(Uri callbackUri)
    {
        if(callbackUri is null || callbackUri.Scheme != Uri.UriSchemeHttp && callbackUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("The CallbackUri must be an absolute URI with HTTP or HTTPS scheme.", nameof(callbackUri));
        }
        CallbackUri = callbackUri;
    }
    public AuthCallbackHandler(
        AuthCallbackOptions options)
    {
        if (options?.CallbackUri is not Uri uri
            || uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("The CallbackUri must be an absolute URI with HTTP or HTTPS scheme.", nameof(AuthCallbackOptions.CallbackUri));
        }
        CallbackUri = uri;
    }
    [ActivatorUtilitiesConstructor]
    public AuthCallbackHandler(
        IOptions<AuthCallbackOptions> options)
    {
        if(options?.Value?.CallbackUri is not Uri uri
            || uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("The CallbackUri must be an absolute URI with HTTP or HTTPS scheme.", nameof(AuthCallbackOptions.CallbackUri));
        }
        CallbackUri = uri;
    }

    public Task HandleRequest(CancellationToken ct, IHttpServerRequest request, string relativePath)
    {
        if (request.Url.AbsolutePath.StartsWith(CallbackUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
        {

            var parameters = request.Url.Query.GetQuery(CallbackUri.AbsolutePath);

            var statusCode = GetStatusCode(parameters); // OAuth specific error handling!

            var result = GetWebAuthenticationResult(statusCode, request.Url.OriginalString);

            _tcs.TrySetResult(result);
            request.SetResponse(
                System.Net.Mime.MediaTypeNames.Text.Plain,
                GetWebAuthenticationResponseMessage(result));
        }

        return Task.CompletedTask;
    }
    protected virtual WebAuthenticationResult GetWebAuthenticationResult(uint statusCode, string requestUriString)
    {
        return statusCode switch
        {
            200 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.Success),
            403 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.UserCancel),
            _ => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.ErrorHttp),
        };
    }
    protected virtual uint GetStatusCode(NameValueCollection parameters)
    {
        if (parameters.Get(OAuthErrorResponseDefaults.ErrorKey) is string error)
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
    protected virtual uint GetStatusCode(IDictionary<string,string> parameters)
    {
        if (parameters.TryGetValue(OAuthErrorResponseDefaults.ErrorKey, out var error))
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
    protected virtual string GetWebAuthenticationResponseMessage(WebAuthenticationResult result)
    {
        return result.ResponseStatus switch
        {
            WebAuthenticationStatus.Success => "Authentication completed successfully - you can close this browser now.",
            WebAuthenticationStatus.UserCancel => "Authentication was cancelled by the user.",
            WebAuthenticationStatus.ErrorHttp => "Authentication failed due to an error.",
            _ => "Authentication completed - you can close this browser now."
        };
    }
    public Task<WebAuthenticationResult> WaitForCallbackAsync()
    {
        return _tcs.Task;
    }
}
