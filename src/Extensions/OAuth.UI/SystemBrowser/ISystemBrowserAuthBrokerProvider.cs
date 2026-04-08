
namespace DevTKSS.Extensions.OAuth.UI.SystemBrowser;

public interface ISystemBrowserAuthBrokerProvider
{
    Task<WebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri, Uri callbackUri, CancellationToken ct);
    void Configure(Action<ServerOptions>? configureServer = null, Action<AuthCallbackHandlerOptions>? configureCallback = null);
    Uri GetCurrentApplicationCallbackUri();
}