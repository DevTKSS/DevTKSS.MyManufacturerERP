//// Import the namespace its defined (the one below) to make it available in the attribute which needs to be above the namespace declaration.
////[assembly:
////    ApiExtension(typeof(IWebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker))]

//using Uno.Disposables;

//namespace DevTKSS.Extensions.OAuth.HttpListenerService;

//public interface IHttpListenerCallbackHandler : IDisposable
//{
//    public Task HandleRequest(HttpListenerCallback request, string relativePath, CancellationToken ct);
//    public Task<WebAuthenticationResult> WaitForCallbackAsync();
//}