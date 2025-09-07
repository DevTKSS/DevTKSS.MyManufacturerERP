using Uno.AuthenticationBroker; // BUG: This is known to cause CS issue: https://github.com/unoplatform/uno/issues/21237
using Uno.Foundation.Extensibility;

[assembly:
    ApiExtension(typeof(IWebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker))]

namespace DevTKSS.Extensions.OAuth.Browser;
public sealed class SystemBrowserAuthBroker
    : IWebAuthenticationBrokerProvider
{
    private readonly ILogger<SystemBrowserAuthBroker> _logger;
    private readonly IHttpServer _server;
    private readonly IAuthCallbackHandler _callbackHandler;
    private readonly IBrowserProvider _browserProvider;
    private Uri? _serverRootUri;
    private Uri ServerRootUri
    {
        get
        {
            if (_serverRootUri is null)
            {
                (_serverRootUri, _) = _server.Start();
            }

            return _serverRootUri;
        }
    }
    public SystemBrowserAuthBroker(
        ILogger<SystemBrowserAuthBroker> logger,
        IBrowserProvider browserProvider,
        IHttpServer httpServer,
        IAuthCallbackHandler callbackHandler)
    {
        _browserProvider = browserProvider;
        _server = httpServer;
        _callbackHandler = callbackHandler;
        _logger = logger;
        _server = httpServer;
    }

    public Uri GetCurrentApplicationCallbackUri()
    {
        if( _callbackHandler.CallbackUri is null)
        {
            // TODO: Check if we can get the callback URI from somewhere else, e.g. from protocol if existing in the target platform.
            throw new InvalidOperationException("The callback URI has not been set. Make sure to call AuthenticateAsync first.");
        }
        return new Uri(ServerRootUri, _callbackHandler.CallbackUri);
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

        CheckWebAuthOptionFlag(options);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Opening browser to authorization endpoint: {AuthUri}", requestUri.ToSafeDisplay()); // TODO: Check if logging the full URI is safe enough.
            _logger.LogInformation("Listening for callback at: {Callback}", callbackUri.ToString());
        }
        var startedServerRootUri = ServerRootUri;
        var callbackHandler = new OAuthCallbackHandler(callbackUri);
        using (_server.RegisterHandler(callbackHandler))
        {
            _browserProvider.OpenBrowser(requestUri);
            var result = await _callbackHandler.WaitForCallbackAsync();
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Authentication flow completed with status: {Status}", result.ResponseStatus);
            }
            return result;

        }
    }
public async Task<WebAuthenticationResult> AuthenticateAsync(
        WebAuthenticationOptions options,
        Uri requestUri,
        CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Starting desktop OAuth authorization code flow.");
        }

        CheckWebAuthOptionFlag(options);
        var startedServerRootUri = ServerRootUri;
        using (_server.RegisterHandler(_callbackHandler))
        {
            _browserProvider.OpenBrowser(requestUri);
            var result = await _callbackHandler.WaitForCallbackAsync();
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Authentication flow completed with status: {Status}", result.ResponseStatus);
            }
            return result;

        }
    }

    private void CheckWebAuthOptionFlag(WebAuthenticationOptions options)
    {
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
    }

    
}
