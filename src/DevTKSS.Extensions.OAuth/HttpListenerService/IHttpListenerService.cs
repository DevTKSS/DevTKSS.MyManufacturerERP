namespace DevTKSS.Extensions.OAuth.HttpListenerService;

public interface IHttpListenerService
{
    public void Start(Uri requestUri, Uri callbackUri);
    public void Stop();
    public Uri GetCallbackUri();
    public IDisposable RegisterHandler(IHttpListenerCallbackHandler handler);
}