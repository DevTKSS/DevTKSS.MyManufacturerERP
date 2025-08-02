using System.Net;
using DevTKSS.MyManufacturerERP.Infrastructure.Entitys;
using FluentValidation;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

internal sealed class SimpleHttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly HttpServerOptions _options;
    public event Func<HttpListenerContext, Task> OnCallbackReceived;
    public SimpleHttpServer(IOptions<HttpServerOptions> config)
    {
        this._options = config.Value;
        _listener = new HttpListener();
        // Set the prefix for the listener
        _listener.Prefixes.Add(config.Value.base)
        // Do not add a prefix here, as callbackPath and slash count are context-dependent.
    }

    public async Task StartAsymc(string baseUri, string callbackPath, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(baseUri, nameof(baseUri));
        ArgumentNullException.ThrowIfNull(callbackPath, nameof(callbackPath));
        
        // Normalize callbackPath: remove leading slashes
        callbackPath = callbackPath?.TrimStart('/') ?? string.Empty;

        // Optionally add trailing slash if callbackPath is not empty and doesn't end with one
        if (!string.IsNullOrEmpty(callbackPath) && !callbackPath.EndsWith('/'))
        {
            callbackPath += '/';
        }
        // Combine base URI and callbackPath
        var urlToListenOn = string.IsNullOrEmpty(callbackPath) ? baseUri : $"{baseUri}{callbackPath}";

        // Add the prefix to the listener
        _listener.Prefixes.Add(urlToListenOn);
        // Start the listener
        _listener.Start();
        _listener.BeginGetContext(OnCallbackReceived, null);
    }

    private void OnCallbackReceived(IAsyncResult ar)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        this._listener?.Stop();
        this._listener?.Close();
    }

    public Func<bool, string> GetBaseUri => (doubleSlash) =>
    {
        string scheme = _options.UseHttps ? "https" : "http";
        int port = _options.Port;
        string domain = _options.Domain;


        string slashes = doubleSlash ? "//" : "/";

        // Build the base URI
        return $"{scheme}:{slashes}{domain}:{port}/";

    };
}