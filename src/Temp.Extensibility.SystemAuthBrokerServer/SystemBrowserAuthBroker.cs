#if HAS_UNO_SKIA
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Security.Authentication.Web;
using Uno.AuthenticationBroker;
using Uno.Foundation.Extensibility;
using Yllibed.HttpServer;
using Yllibed.HttpServer.Handlers;
using Temp.Extensibility.DesktopAuthBroker;
[assembly:
    ApiExtension(typeof(IWebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker))]

namespace Temp.Extensibility.DesktopAuthBroker;

public sealed class SystemBrowserAuthBroker : IWebAuthenticationBrokerProvider
{
    private Server? _server;

    private Uri? _serverRootUri;

    public Uri GetCurrentApplicationCallbackUri()
    {
        return new Uri(EnsureServerStarted().RootUri, "/auth-callback");
    }

    public async Task<WebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions _options, Uri requestUri,
        Uri callbackUri, CancellationToken ct)
    {
        if (_options.HasFlag(WebAuthenticationOptions.SilentMode))
        {
            throw new NotSupportedException("SilentMode is not supported by this broker.");
        }

        if (_options.HasFlag(WebAuthenticationOptions.UseTitle))
        {
            throw new NotSupportedException("UseTitle is not supported by this broker.");
        }

        if (_options.HasFlag(WebAuthenticationOptions.UseHttpPost))
        {
            throw new NotSupportedException("UseHttpPost is not supported by this broker.");
        }

        if (_options.HasFlag(WebAuthenticationOptions.UseCorporateNetwork))
        {
            throw new NotSupportedException("UseCorporateNetwork is not supported by this broker.");
        }

        var (server, _) = EnsureServerStarted();
        var authCallbackHandler = new AuthCallbackHandler(callbackUri);
        using (server.RegisterHandler(authCallbackHandler))
        {
            OpenBrowser(requestUri);
            return await authCallbackHandler.WaitForCallbackAsync();
        }
    }

    private (Server Server, Uri RootUri) EnsureServerStarted()
    {
        if (_server is null || _serverRootUri is null)
        {
            _server = new Server();
            (_serverRootUri, _) = _server.Start();
        }

        return (_server, _serverRootUri);
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
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
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


    private sealed class AuthCallbackHandler(Uri callbackUri) : IHttpHandler
    {
        private readonly TaskCompletionSource<WebAuthenticationResult> _tcs = new();

        public Task HandleRequest(CancellationToken ct, IHttpServerRequest request, string relativePath)
        {
            if (request.Url.AbsolutePath.StartsWith(callbackUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                var result = new WebAuthenticationResult(
                    request.Url.OriginalString,
                    200,
                    WebAuthenticationStatus.Success);

                _tcs.TrySetResult(result);
                request.SetResponse("text/plain", "Auth completed - you can close this browser now.");
            }

            return Task.CompletedTask;
        }

        public Task<WebAuthenticationResult> WaitForCallbackAsync()
        {
            return _tcs.Task;
        }
    }

}
#endif