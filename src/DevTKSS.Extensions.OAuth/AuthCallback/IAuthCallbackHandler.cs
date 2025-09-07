namespace DevTKSS.Extensions.OAuth.AuthCallback;

public interface IAuthCallbackHandler : IHttpHandler
{
    public string Name { get; }
    public Uri CallbackUri { get; }
    public Task<WebAuthenticationResult> WaitForCallbackAsync();
}
