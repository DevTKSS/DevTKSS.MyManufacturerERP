using System.Collections.Immutable;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// references used:
// - https://github.com/hyun4545/OAuthDesktopApp/blob/master/OAuthDesktopApp/KeycloakService.cs
// - https://github.com/carldebilly/Yllibed.HttpServer/blob/master/Yllibed.HttpServer/Server.cs
namespace Temp.Extensibility.DesktopAuthBroker;

public class HttpListenerServer : IHttpListenerServer
{
    private readonly HttpListener _listener = new();
    private ImmutableList<IHttpListenerHandler> _handlers = ImmutableList<IHttpListenerHandler>.Empty;
    private readonly ILogger<HttpListenerServer> _logger;
    private readonly ServerOptions? _options;
    private CancellationTokenSource? _cts;
    private volatile bool _isRunning;
    public HttpListenerServer(
        ILogger<HttpListenerServer> logger,
        IOptions<ServerOptions>? options = null)
    {
        _logger = logger;
        _options = options?.Value;
    }

    public void Start()
    {
        ArgumentNullException.ThrowIfNull(_options, nameof(_options));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_options.RelativeCallbackUri, nameof(_options.RelativeCallbackUri));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_options.RootUri, nameof(_options.RootUri));
        if (!Uri.TryCreate(_options.RootUri, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid root URI", nameof(_options.RootUri));
        }
        if (!Uri.TryCreate(_options.RelativeCallbackUri, UriKind.Relative, out _))
        {
            throw new ArgumentException("Invalid relative callback URI", nameof(_options.RelativeCallbackUri));
        }
        Start(_options.RootUri!,_options.RelativeCallbackUri!);
    }
    public void Start(string rootUri, string relativeCallbackUri)
    {
        _cts = new CancellationTokenSource();
        if(HttpListener.IsSupported == false)
        {
            throw new PlatformNotSupportedException("HttpListener is not supported on this platform.");
        }
        _listener.Prefixes.Clear();
        var baseUri = new Uri(rootUri, UriKind.Absolute);
        var fullUri = new Uri(baseUri, relativeCallbackUri);
        var absoluteUri = fullUri.AbsoluteUri.EndsWith("/") ? fullUri.AbsoluteUri : fullUri.AbsoluteUri + "/";
        _listener.Prefixes.Add(absoluteUri);
        _listener.Start();
        _isRunning = true;
        _logger?.LogInformation("Listening for requests on {fullUri}", fullUri);
        BeginGetContextLoop();
    }

    public void Stop()
    {
        _isRunning = false;
        _cts?.Cancel();
        _listener.Stop();
        _logger?.LogInformation("HttpListener stopped.");
    }

    private void BeginGetContextLoop()
    {
        if (_isRunning && _listener.IsListening)
        {
            try
            {
                _listener.BeginGetContext(OnContextReceived, null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in BeginGetContextLoop.");
            }
        }
    }

    private void OnContextReceived(IAsyncResult ar)
    {
        if (!_isRunning || !_listener.IsListening) return;
        HttpListenerContext context = null!;
        try
        {
            context = _listener.EndGetContext(ar);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in OnContextReceived.");
            return;
        }
        BeginGetContextLoop(); // Register next callback
        _ = Task.Run(() => HandleRequest(context));
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        foreach (var handler in _handlers)
        {
            await handler.HandleRequest(context);
            if (!context.Response.OutputStream.CanWrite)
                break;
        }
        if (context.Response.OutputStream.CanWrite)
        {
            var buffer = Encoding.UTF8.GetBytes($"Requested address {context.Request.Url} not found.");
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 404;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }
    }

    public IDisposable RegisterHandler(IHttpListenerHandler handler)
    {
        ImmutableInterlocked.Update(ref _handlers, (list, h) => list.Add(h), handler);
        return new HandlerDisposable(this, handler);
    }

    private void UnregisterHandler(IHttpListenerHandler handler)
    {
        ImmutableInterlocked.Update(ref _handlers, (list, h) => list.Remove(h), handler);
    }

    private class HandlerDisposable : IDisposable
    {
        private readonly HttpListenerServer _server;
        private readonly IHttpListenerHandler _handler;
        private bool _disposed;
        public HandlerDisposable(HttpListenerServer server, IHttpListenerHandler handler)
        {
            _server = server;
            _handler = handler;
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _server.UnregisterHandler(_handler);
                _disposed = true;
            }
        }
    }
}
