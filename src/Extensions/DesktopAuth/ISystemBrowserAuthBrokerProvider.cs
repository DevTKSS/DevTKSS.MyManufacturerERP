namespace DevTKSS.Extensions.Uno.Authentication.Desktop;

public interface ISystemBrowserAuthBrokerProvider
{
    Task<WebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri, Uri callbackUri, CancellationToken ct);
    Uri GetCurrentApplicationCallbackUri();
}