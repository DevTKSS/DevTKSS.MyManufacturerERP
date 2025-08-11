#if DESKTOP || HAS_UNO_SKIA
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// Import the namespace its defined (the one below) to make it available in the attribute which needs to be above the namespace declaration.
using Temp.Extensibility.DesktopAuthBroker;
using Uno.AuthenticationBroker;
using Uno.Foundation.Extensibility;
using Windows.Security.Authentication.Web;

[assembly:
    ApiExtension(typeof(IWebAuthenticationBrokerProvider),typeof(SystemBrowserAuthBroker))]

namespace Temp.Extensibility.DesktopAuthBroker;
public sealed class SystemBrowserAuthBroker : IWebAuthenticationBrokerProvider
{
    private readonly IHttpListenerServer _server;
    private Uri _serverRootUri;
    private string? _relativeCallbackUri;
    private readonly ILogger<SystemBrowserAuthBroker> _logger;

    public SystemBrowserAuthBroker(
        ILogger<SystemBrowserAuthBroker> logger,
        IHttpListenerServer server,
        IOptions<ServerOptions> serverOptions)
    {
        _logger = logger;
        _server = server;
        ArgumentNullException.ThrowIfNull(serverOptions.Value.RootUri, nameof(serverOptions.Value.RootUri));
        _serverRootUri = new Uri(serverOptions.Value.RootUri,UriKind.Absolute);
    }
    public Uri GetCurrentApplicationCallbackUri()
    {
        EnsureServerStarted();
        return new Uri(_serverRootUri, _relativeCallbackUri);
    }
    public void SetCurrentApplicationCallbackUri(string callbackUri)
    {
        if (callbackUri != null  && Uri.TryCreate(callbackUri, UriKind.Relative, out var checkedUri))
        {
            _relativeCallbackUri = checkedUri!.PathAndQuery;
        }
       throw new ArgumentNullException(nameof(callbackUri));
    }
    public async Task<WebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri,
        Uri callbackUri, CancellationToken ct)
    {
        if(_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Authenticating started with requestUri: {RequestUri}, callbackUri: {CallbackUri}, options: {Options}",
                requestUri,
                callbackUri,
                options);
        }
        if (options.HasFlag(WebAuthenticationOptions.SilentMode))
        {
            throw new NotSupportedException("SilentMode is not supported by this broker.");
        }

        if (options.HasFlag(WebAuthenticationOptions.UseTitle))
        {
            throw new NotSupportedException("UseTitle is not supported by this broker.");
        }

        if (options.HasFlag(WebAuthenticationOptions.UseHttpPost))
        {
            throw new NotSupportedException("UseHttpPost is not supported by this broker.");
        }

        if (options.HasFlag(WebAuthenticationOptions.UseCorporateNetwork))
        {
            throw new NotSupportedException("UseCorporateNetwork is not supported by this broker.");
        }

        EnsureServerStarted();
        var authCallbackHandler = new AuthCallbackHandler(callbackUri);
        using (_server.RegisterHandler(authCallbackHandler))
        {
            OpenBrowser(requestUri);
            return await authCallbackHandler.WaitForCallbackAsync();
        }
    }

    private void EnsureServerStarted()
    {
        _server.Start();
    }

    public static void OpenBrowser(Uri uri)
    {
        var url = uri.AbsoluteUri;
        try
        {
            Process.Start(url);
        }
        catch
        {
            //  hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    private sealed class AuthCallbackHandler : IHttpListenerHandler
    {
        private readonly Uri _callbackUri;
        private readonly TaskCompletionSource<WebAuthenticationResult> _tcs = new();

        public AuthCallbackHandler(Uri callbackUri)
        {
            _callbackUri = callbackUri;
        }

        public async Task HandleRequest(HttpListenerContext context)
        {
            if (context.Request.Url != null
                && context.Request.Url.AbsolutePath.StartsWith(_callbackUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                var result = new WebAuthenticationResult(
                    context.Request.Url.OriginalString,
                    200,
                    WebAuthenticationStatus.Success);

                _tcs.TrySetResult(result);
                var buffer = Encoding.UTF8.GetBytes("Auth completed - you can close this browser now.");
                context.Response.ContentType = "text/plain";
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();
            }
            else
            {
                // If the request is not for the callback URI, we ignore it.
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }

        public Task<WebAuthenticationResult> WaitForCallbackAsync()
        {
            return _tcs.Task;
        }
    }
}
#endif