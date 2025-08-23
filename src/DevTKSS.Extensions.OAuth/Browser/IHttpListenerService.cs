namespace DevTKSS.Extensions.OAuth.Browser;

public interface IHttpListenerService
{
    public Task<WebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri, Uri callbackUri, CancellationToken ct);
    public Uri GetCurrentApplicationCallbackUri();
}