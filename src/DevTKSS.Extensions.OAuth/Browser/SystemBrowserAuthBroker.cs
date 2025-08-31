using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DevTKSS.Extensions.OAuth.Browser;


// Import the namespace its defined (the one below) to make it available in the attribute which needs to be above the namespace declaration.
using Uno.AuthenticationBroker;
using Uno.Foundation.Extensibility;

//[assembly:
//    ApiExtension(typeof(IWebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker))]

namespace DevTKSS.Extensions.OAuth.Browser;
public sealed class SystemBrowserAuthBroker : ISystemBrowserAuthBrokerProvider
//: IWebAuthenticationBrokerProvider
{
    private readonly IHttpListenerService _httpService;
    private readonly IBrowserProvider _browserProvider;
    private readonly ILogger _logger;

    public SystemBrowserAuthBroker(
        IBrowserProvider browserProvider,
        IHttpListenerService httpService,
        ILogger logger)
    {
        _httpService = httpService;
        _browserProvider = browserProvider;
        _logger = logger.ForContext<SystemBrowserAuthBroker>();
    }

    public Uri GetCurrentApplicationCallbackUri()
    {
        return _httpService.GetCallbackUri();
    }

    public async Task<WebAuthenticationResult> AuthenticateAsync(
        WebAuthenticationOptions options,
        Uri requestUri,
        Uri callbackUri,
        CancellationToken ct)
    {
        if (options.HasFlag(WebAuthenticationOptions.SilentMode))
        {
            throw new NotSupportedException("SilentMode is not supported by this broker.");
        }
#pragma warning disable IDE0079 // Remove unnecessary suppression of the warning.
#pragma warning disable Uno0001 // WebAuthenticationOptions.UseTitle is not supported to be used in Uno Platform. We know about this, thats why we throw those exceptions in there.
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
#pragma warning restore Uno0001 // WebAuthenticationOptions.UseTitle is not supported to be used in Uno Platform.
#pragma warning restore IDE0079 // Remove unnecessary suppression of the warning.

        var authCallbackHandler = new AuthCallbackHandler(callbackUri);
        using (_httpService.RegisterHandler(authCallbackHandler))
        {
            _httpService.Start(requestUri, callbackUri);
            return await authCallbackHandler.WaitForCallbackAsync();
        }
    }

    //public static void OpenBrowser(Uri uri)
    //{
    //    var url = uri.AbsoluteUri;
    //    try
    //    {
    //        Process.Start(url);
    //    }
    //    catch
    //    {
    //        // hack because of this: https://github.com/dotnet/corefx/issues/10361
    //        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //        {
    //            url = url.Replace("&", "^&");
    //            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
    //        }
    //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    //        {
    //            Process.Start("xdg-open", url);
    //        }
    //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
    //                 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
    //        {
    //            Process.Start("open", url);
    //        }
    //        else
    //        {
    //            throw;
    //        }
    //    }
    //}


    private sealed class AuthCallbackHandler(Uri callbackUri) : IHttpListenerCallbackHandler
    {
        private readonly TaskCompletionSource<WebAuthenticationResult> _tcs = new();

        public async Task HandleRequest(HttpListenerCallback callback, string relativePath, CancellationToken ct)
        {
            WebAuthenticationResult result;
            if (callback.Request.Url is not null && callback.Request.Url.AbsolutePath.StartsWith(callbackUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                uint statusCode = 200;
                var requestUriString = callback.Request.Url?.ToString() ?? string.Empty;
                // Checks for errors.
                statusCode = GetStatusCode(callback.Request.QueryString, requestUriString);
                result = statusCode switch
                {
                    200 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.Success),
                    403 => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.UserCancel),
                    _ => new WebAuthenticationResult(requestUriString, statusCode, WebAuthenticationStatus.ErrorHttp),
                };

                _tcs.TrySetResult(result);
                await callback.SetResponseAsync(
                    "Auth completed - you can close this browser now.",
                    System.Net.Mime.MediaTypeNames.Text.Plain,
                    statusCode: 200,
                    cancellationToken: ct);
            }
        }

        private static uint GetStatusCode(NameValueCollection queryString, string requestUriString)
        {
            if (queryString.Get(OAuthErrorResponseDefaults.ErrorKey) is string error)
            {
                var errorDescription = queryString.Get(OAuthErrorResponseDefaults.ErrorDescriptionKey);
                var errorUri = queryString.Get(OAuthErrorResponseDefaults.ErrorUriKey);
                Log.ForContext<AuthCallbackHandler>().Error("OAuth authorization error: '{error}', error_description: '{errorDescription}', error_uri: '{errorUri}'", error, errorDescription, errorUri);

                return error switch
                {
                    OAuthErrorResponseDefaults.AccessDenied => 403,
                    OAuthErrorResponseDefaults.InvalidClient or OAuthErrorResponseDefaults.UnauthorizedClient or OAuthErrorResponseDefaults.InvalidScope => 401,
                    OAuthErrorResponseDefaults.TemporarilyUnavailable => 503,
                    OAuthErrorResponseDefaults.UnsupportedGrantType => 500,
                    _ => 400 // For all others: Bad Request
                };

            }

            return 200;
        }

        public Task<WebAuthenticationResult> WaitForCallbackAsync()
        {
            return _tcs.Task;
        }
    }
}
public interface IHttpListenerCallbackHandler
{
    public Task HandleRequest(HttpListenerCallback request, string relativePath, CancellationToken ct);
    public Task<WebAuthenticationResult> WaitForCallbackAsync();
}