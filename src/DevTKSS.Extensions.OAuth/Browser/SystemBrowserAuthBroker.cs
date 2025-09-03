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
    private readonly ILogger<SystemBrowserAuthBroker> _logger;
    private readonly IHttpListenerCallbackHandler _callbackHandler;

    public SystemBrowserAuthBroker(
        IBrowserProvider browserProvider,
        IHttpListenerService httpService,
        ILogger<SystemBrowserAuthBroker> logger,
        IHttpListenerCallbackHandler callbackHandler)
    {
        _httpService = httpService;
        _logger = logger;
        _callbackHandler = callbackHandler;
    }

    public Uri GetCurrentApplicationCallbackUri()
    {
        var uri = _httpService.GetCallbackUri();
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Broker provided callback URI: {Callback}", uri.ToSafeDisplay());
        }
        return uri;
    }

    public async Task<WebAuthenticationResult> AuthenticateAsync(
        WebAuthenticationOptions options,
        Uri requestUri,
        Uri callbackUri,
        CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Starting desktop OAuth authorization code flow.");
        }

        if (options.HasFlag(WebAuthenticationOptions.SilentMode))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("SilentMode is not supported by this broker.");
            }
            throw new NotSupportedException("SilentMode is not supported by this broker.");
        }
#pragma warning disable IDE0079 // Remove unnecessary suppression of the warning.
#pragma warning disable Uno0001 // WebAuthenticationOptions.UseTitle is not supported to be used in Uno Platform. We know about this, thats why we throw those exceptions in there.
        if (options.HasFlag(WebAuthenticationOptions.UseTitle))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("UseTitle is not supported by this broker.");
            }
            throw new NotSupportedException("UseTitle is not supported by this broker.");
        }

        if (options.HasFlag(WebAuthenticationOptions.UseHttpPost))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("UseHttpPost is not supported by this broker.");
            }
            throw new NotSupportedException("UseHttpPost is not supported by this broker.");
        }

        if (options.HasFlag(WebAuthenticationOptions.UseCorporateNetwork))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("UseCorporateNetwork is not supported by this broker.");
            }
            throw new NotSupportedException("UseCorporateNetwork is not supported by this broker.");
        }
#pragma warning restore Uno0001 // WebAuthenticationOptions.UseTitle is not supported to be used in Uno Platform.
#pragma warning restore IDE0079 // Remove unnecessary suppression of the warning.

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Opening browser to authorization endpoint: {AuthUri}", requestUri.ToSafeDisplay());
            _logger.LogInformation("Listening for callback at: {Callback}", callbackUri.ToSafeDisplay());
        }
;
        using (_httpService.RegisterHandler(_callbackHandler))
        {
            try
            {
                _httpService.Start(requestUri, callbackUri);

                using (ct.Register(() =>
                {
                    try { _httpService.Stop(); } catch (Exception ex) { if (_logger.IsEnabled(LogLevel.Warning)) { _logger.LogWarning(ex, "Error while stopping listener on cancellation"); } }
                }))
                {
                    var result = await _callbackHandler.WaitForCallbackAsync().ConfigureAwait(false);
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Authentication flow completed with status: {Status}", result.ResponseStatus);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex, "Error during authentication flow");
                }
                return new WebAuthenticationResult(null, 0, WebAuthenticationStatus.ErrorHttp);
            }
        }
    }
}
