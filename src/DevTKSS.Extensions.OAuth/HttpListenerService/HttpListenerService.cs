//using System.Collections.Immutable;
//using System.Net.Mime;
//using Uno.Disposables;
//using Yllibed.HttpServer.Extensions;

//namespace DevTKSS.Extensions.OAuth.HttpListenerService;
//public class HttpListenerService : IHttpListenerService
//{
//    private readonly CancellationTokenSource _cts = new();
//    private readonly ILogger<HttpListenerService> _logger;
//    private readonly IBrowserProvider _browserProvider;
//    private readonly AuthCallbackOptions _callbackOptions;
//    private ImmutableList<IHttpListenerCallbackHandler> _handlers = ImmutableList<IHttpListenerCallbackHandler>.Empty;
//    private HttpListener? _httpListener;

//    public HttpListenerService(
//        ILogger<HttpListenerService> logger,
//        IBrowserProvider browserProvider,
//        IOptions<AuthCallbackOptions>? options = null)
//    {
//        _logger = logger;
//        _browserProvider = browserProvider;
//        _callbackOptions = options?.Value ?? new AuthCallbackOptions();
//    }

//    public Uri GetCallbackUri()
//    {
//        // If OAuthOptions.RedirectUri is provided, honor it strictly
//        var configuredRedirect = _callbackOptions?.CallbackUri?.OriginalString;
//        if (!string.IsNullOrWhiteSpace(configuredRedirect)
//            && Uri.TryCreate(configuredRedirect, UriKind.Absolute, out var oauthRedirect)
//            && (oauthRedirect.Scheme == Uri.UriSchemeHttp || oauthRedirect.Scheme == Uri.UriSchemeHttps))
//        {
//            if (_logger.IsEnabled(LogLevel.Debug))
//            {
//                _logger.LogDebug("Using OAuthOptions RedirectUri as callback: {Callback}", oauthRedirect.ToSafeDisplay());
//            }
//            return oauthRedirect;
//        }

//        var callbackUri = _callbackOptions.CallbackUri;

//        if (callbackUri is null || !callbackUri.IsAbsoluteUri)
//        {
//            if (_logger.IsEnabled(LogLevel.Error))
//            {
//                _logger.LogError("Invalid RedirectUri in OAuthOptions: {UriString}", callbackUri);
//            }
//            throw new ArgumentException("The RedirectUri is not a valid absolute URI.");
//        }
        
//        // Only auto-assign a port when explicitly requested (Port == 0) for loopback URIs.
//        if (callbackUri.IsLoopback
//            && (callbackUri.Scheme == Uri.UriSchemeHttp || callbackUri.Scheme == Uri.UriSchemeHttps)
//            && callbackUri.Port == 0)
//        {
//            if (_logger.IsEnabled(LogLevel.Debug))
//            {
//                _logger.LogDebug("No port specified for loopback URI. Allocating a free port.");
//            }
//            var listener = new TcpListener(IPAddress.Loopback, 0);
//            listener.Start();
//            var resultingPort = ((IPEndPoint)listener.LocalEndpoint).Port;
//            listener.Stop();

//            var builder = new UriBuilder(callbackUri)
//            {
//                Port = resultingPort
//            };
//            callbackUri = builder.Uri;
//            if (_logger.IsEnabled(LogLevel.Information))
//            {
//                _logger.LogInformation("Assigned loopback port {Port} for callback URI.", resultingPort);
//            }
//        }

//        if (_logger.IsEnabled(LogLevel.Debug))
//        {
//            _logger.LogDebug("Final callback URI: {Callback}", callbackUri.ToSafeDisplay());
//        }
//        return callbackUri;
//    }

//    private static Uri EnsurePrefixUriFormat(Uri uri)
//    {
//        var builder = new UriBuilder(uri) { Query = string.Empty, Fragment = string.Empty };
//        var cleaned = builder.Uri;
//        if (!cleaned.AbsoluteUri.EndsWith('/'))
//        {
//            return new Uri(cleaned.AbsoluteUri + "/");
//        }
//        return cleaned;
//    }

//    public void Start(Uri requestUri, Uri callbackUri)
//    {
//        if (!HttpListener.IsSupported)
//            throw new NotSupportedException("HttpListener is not supported on this platform.");

//        if (!(callbackUri.Scheme == Uri.UriSchemeHttp || callbackUri.Scheme == Uri.UriSchemeHttps))
//        {
//            if (_logger.IsEnabled(LogLevel.Error))
//            {
//                _logger.LogError("Unsupported callback URI scheme: {Scheme}", callbackUri.Scheme);
//            }
//            throw new NotSupportedException($"Only http/https callback URIs are supported by this implementation. Provided: {callbackUri.Scheme}");
//        }

//        StopInternal();

//        var prefixUri = EnsurePrefixUriFormat(callbackUri);
//        var prefix = prefixUri.AbsoluteUri;

//        _httpListener = new HttpListener();
//        _httpListener.Prefixes.Add(prefix);
//        if (_logger.IsEnabled(LogLevel.Information))
//        {
//            _logger.LogInformation("Starting HTTP listener at {Prefix}", prefix);
//        }
//        _httpListener.Start();

//        if (_logger.IsEnabled(LogLevel.Information))
//        {
//            _logger.LogInformation("Opening system browser to begin authentication. Request: {RequestUri}", requestUri.ToSafeDisplay());
//        }
//        _browserProvider.OpenBrowser(requestUri);

//        _ = Task.Run(() => HandleIncomingRequests(_httpListener, _cts.Token));
//    }

//    public void Stop() => StopInternal();

//    private async Task HandleIncomingRequests(HttpListener httpListener, CancellationToken ct)
//    {
//        try
//        {
//            HttpListenerContext context;
//            try
//            {
//                context = await httpListener.GetContextAsync().ConfigureAwait(false);
//            }
//            catch (HttpListenerException) when (!httpListener.IsListening)
//            {
//                if (_logger.IsEnabled(LogLevel.Debug))
//                {
//                    _logger.LogDebug("Listener stopped before receiving a request.");
//                }
//                return;
//            }
//            catch (ObjectDisposedException)
//            {
//                if (_logger.IsEnabled(LogLevel.Debug))
//                {
//                    _logger.LogDebug("Listener disposed before receiving a request.");
//                }
//                return;
//            }

//            var relativePath = context.Request.Url?.AbsolutePath ?? string.Empty;
//            if (_logger.IsEnabled(LogLevel.Information))
//            {
//                _logger.LogInformation("Received auth callback at path {Path} from {Remote}", relativePath, context.Request.RemoteEndPoint);
//            }

//            var callback = new HttpListenerCallback(context);

//            foreach (var handler in _handlers)
//            {
//                try
//                {
//                    await handler.HandleRequest(callback, relativePath, ct).ConfigureAwait(false);
//                }
//                catch (Exception ex)
//                {
//                    if (_logger.IsEnabled(LogLevel.Error))
//                    {
//                        _logger.LogError(ex, "Error in handler {HandlerType} for request {RequestUrl}", handler.GetType().FullName, context.Request.Url?.ToSafeDisplay());
//                    }
//                }
//            }

//            if (!callback.IsResponseSet)
//            {
//                if (_logger.IsEnabled(LogLevel.Warning))
//                {
//                    _logger.LogWarning("No handler produced a response. Returning 404.");
//                }
//                await callback.SetResponseAsync(
//                    "Not Found",
//                    MediaTypeNames.Text.Plain,
//                    statusCode: 404,
//                    cancellationToken: ct).ConfigureAwait(false);
//            }
//            else
//            {
//                if (_logger.IsEnabled(LogLevel.Debug))
//                {
//                    _logger.LogDebug("Response sent to browser.");
//                }
//            }
//        }
//        finally
//        {
//            if (_logger.IsEnabled(LogLevel.Information))
//            {
//                _logger.LogInformation("Stopping HTTP listener.");
//            }
//            StopInternal();
//        }
//    }

//    public IDisposable RegisterHandler(IHttpListenerCallbackHandler handler)
//    {
//        if (_logger.IsEnabled(LogLevel.Debug))
//        {
//            _logger.LogDebug("Registering callback handler {HandlerType}", handler.GetType().FullName);
//        }
//        ImmutableInterlocked.Update(ref _handlers, (list, h) => list.Add(h), handler);

//        return handler.DisposeWith<IHttpListenerCallbackHandler>(h => // seems like I have to again copy the extension code just to replicate the behavior
//        {
//            ImmutableInterlocked.Update(ref _handlers, (list, h2) => list.Remove(h2), h);
//            (h as IDisposable)?.Dispose();
//        });

//    }

//    private void StopInternal()
//    {
//        try
//        {
//            if (_httpListener is { IsListening: true })
//            {
//                _httpListener.Stop();
//            }
//            _httpListener?.Close();
//        }
//        catch (Exception ex)
//        {
//            if (_logger.IsEnabled(LogLevel.Error))
//            {
//                _logger.LogError(ex, "Error while stopping HTTP listener");
//            }
//        }
//        finally
//        {
//            _httpListener = null;
//        }
//    }

//    public void Dispose()
//    {
//        _cts.Cancel();
//        StopInternal();
//    }
//}
