
namespace DevTKSS.Extensions.OAuth.Browser;

public interface ISystemBrowserAuthBrokerProvider
{
    Task<WebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri, Uri callbackUri, CancellationToken ct);
    Uri GetCurrentApplicationCallbackUri();
}