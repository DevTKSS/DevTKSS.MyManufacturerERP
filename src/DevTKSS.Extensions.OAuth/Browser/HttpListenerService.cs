using System.Collections.Immutable;
using System.Net.Mime;
using Yllibed.HttpServer;
using Yllibed.HttpServer.Extensions;
using Yllibed.HttpServer.Handlers;

namespace DevTKSS.Extensions.OAuth.Browser;
public class HttpListenerService : IHttpListenerService
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;
    private readonly IBrowserProvider _browserProvider;
    private readonly ServerOptions _serverOptions;
    private ImmutableList<IHttpListenerCallbackHandler> _handlers = ImmutableList<IHttpListenerCallbackHandler>.Empty;
    private ImmutableArray<HttpListenerCallback> _requests = [];
    public HttpListenerService(
        ILogger logger,
        IBrowserProvider browserProvider,
        IOptions<ServerOptions>? options = null)
    {
        _logger = logger.ForContext<HttpListenerService>();
        _browserProvider = browserProvider;
        _serverOptions = options?.Value ?? new ServerOptions();
    }

    public Uri GetCallbackUri()
    {
        var uriString = _serverOptions.ToString();

        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var callbackUri))
        {
            throw new ArgumentException("The RedirectUri is not a valid absolute URI.");
        }
        if (_serverOptions.UriFormat == UriFormatMode.Standard
            && callbackUri.IsLoopback
            && (callbackUri.Scheme == Uri.UriSchemeHttp || callbackUri.Scheme == Uri.UriSchemeHttps)
            && callbackUri.Port == 0)
        { 
            var listener = new TcpListener(IPAddress.Loopback, 0);

            listener.Start();
            var resultingPort = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            var builder = new UriBuilder(callbackUri)
            {
                Port = resultingPort
            };
            return builder.Uri;
        }
        return callbackUri;

    }
    /// <summary>
    /// <summary>
    /// Authenticates the user by starting an HTTP listener on the specified callback URI, opening the browser to the OAuth request URI,
    /// and waiting for the OAuth authorization response. Handles the OAuth redirect, processes any errors, and returns the authentication result.
    /// </summary>
    /// <param name="options">The web authentication options to use for the authentication process.</param>
    /// <param name="callbackUri">The callback URI where the HTTP listener will wait for the OAuth response.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the authentication process to complete.</param>
    /// <returns>
    /// A <see cref="Task{WebAuthenticationResult}"/> representing the asynchronous operation, containing the result of the authentication process.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if <see cref="HttpListener"/> is not supported on the current platform.</exception>
    public void Start(Uri requestUri, Uri callbackUri)
    {
        if (!HttpListener.IsSupported) 
            throw new NotSupportedException("HttpListener is not supported on this platform.");
        
        using var httpListener = new HttpListener();
        httpListener.Prefixes.Add(callbackUri.AbsoluteUri);
        _logger.Information($"Listening on {callbackUri.AbsoluteUri}...");
        httpListener.Start();

        _browserProvider.OpenBrowser(requestUri);

        try
        {
            
            _ = Task.Run(() => HandleIncomingRequests(httpListener, _cts.Token));
        }
        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
        {
            _logger.Information("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during StartAsync");
            throw;
        }
        finally
        {
            httpListener.Stop();
        }
    }
    private async Task HandleIncomingRequests(HttpListener httpListener, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var context = await httpListener.GetContextAsync().ConfigureAwait(true);
            var relativePath = context.Request.Url?.AbsolutePath ?? string.Empty;
            HttpListenerCallback? callback = null;
            
            callback = new HttpListenerCallback(
                context,
                onReady: HandleRequest,
                onCompletedOrDisconnected:OnCompletedOrDisconnected,
                ct);
            void OnCompletedOrDisconnected() => ImmutableInterlocked.Update(ref _requests, (list, r2) => list.Remove(r2), callback);
            ImmutableInterlocked.Update(ref _requests, (list, r) => list.Add(r), callback);

            foreach (var handler in _handlers)
            {
                try
                {
                    await handler.HandleRequest(callback, relativePath, ct);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in handler {HandlerType} for request {RequestUrl}", handler.GetType().FullName, context.Request.Url);
                }
            }

            if (!callback.IsResponseSet)
            {
                await callback.SetResponseAsync(
                    "Not Found",
                    MediaTypeNames.Text.Plain,
                    statusCode: 404,
                    cancellationToken: ct);
            }
            break;
        }
    }
    public IDisposable RegisterHandler(IHttpListenerCallbackHandler handler)
    {
        ImmutableInterlocked.Update(ref _handlers, (list, h) => list.Add(h), handler);

        return Disposable.Create(handler,h =>
        {
            ImmutableInterlocked.Update(ref _handlers, (list, h2) => list.Remove(h2), h);
            (h as IDisposable)?.Dispose();
        });

    }
    private async Task HandleRequest(HttpListenerCallback callback, CancellationToken cancellationToken)
    {
        if (!callback.IsResponseSet)
        {

            await callback.SetResponseAsync(
                "Auth completed - you can close this browser now.",
                MediaTypeNames.Text.Plain,
                statusCode: 200,
                cancellationToken: cancellationToken);
        }
    }
    public void Dispose() => _cts.Cancel();
}
