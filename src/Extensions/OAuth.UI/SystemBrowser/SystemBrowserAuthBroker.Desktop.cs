// [assembly: ApiExtension(typeof(WebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker), operatingSystemCondition: "Windows")]
// NOTE: The File extension is set to .Desktop.cs to only include it in Desktop builds but making it available to the non Desktop limited OAuthNavigationService
using System.Net;
using DevTKSS.Extensions.OAuth.Providers;
using Yllibed.HttpServer.Extensions;
using Yllibed.HttpServer.Handlers.Uno.Extensions;

namespace DevTKSS.Extensions.OAuth.UI.SystemBrowser;

public sealed class SystemBrowserAuthBroker()
    : ISystemBrowserAuthBrokerProvider   // IWebAuthenticationBrokerProvider  // TODO: Implement the original Uno Interface if anyone knows, how to successfully set the ApiExtension attribute correctly! Docs are not matching even the API!
{
    private readonly IHttpServer? _server;
    private readonly IAuthCallbackHandler? _callbackHandler;

    /// <summary>
    /// Gets or initializes the used server options.<br/>
    /// Defaults to http://localhost:5001 binding to loopback address.
    /// </summary>
    /// <value>The server options.</value>
    /// <remarks>
    /// 1. <see cref="ServerOptions"/> can not be configured to use HTTPS because of <see cref="Yllibed.HttpServer.Server"/> uses TCPListener which does not support HTTPS natively.<br/>
    /// 2. If you not use DI but want to configure the <see cref="ServerOptions"/>, you must do this before the first call to <see cref="AuthenticateAsync"/> or <see cref="GetCurrentApplicationCallbackUri"/>.
    /// </remarks>
    private ServerOptions ServerOptions
    {
        get
        {
            field ??= new ServerOptions()
            {
                Hostname4 = "localhost",
                Port = 5001,
                BindAddress4 = IPAddress.Loopback
            };
            return field;
        }
        set;
    }
    private AuthCallbackHandlerOptions CallbackHandlerOptions
    {
        get
        {
            field ??= new AuthCallbackHandlerOptions()
            {
                CallbackUri = ServerOptions.ToUri4("/callback")?.ToString()
            };
            return field;
        }
        set;
    }
    public void Configure( // TODO: potentially obsolete, check if we have a different option while staying with a new() ctor
        Action<ServerOptions>? configureServer = default,
        Action<AuthCallbackHandlerOptions>? configureCallback = default)
    {
        configureServer?.Invoke(ServerOptions);
        configureCallback?.Invoke(CallbackHandlerOptions);
    }
    private Uri? _serverRootUri;

    [ActivatorUtilitiesConstructor]
    public SystemBrowserAuthBroker(
        IHttpServer server,
        IAuthCallbackHandler callbackHandler) 
        : this()
    {
        _server = server;
        _callbackHandler = callbackHandler;
    }
    private (Uri RootUri, IAuthCallbackHandler Handler) EnsureServerStarted()
    {
        if (_server is null || _callbackHandler is null)
        { 
            var serviceProvider = new ServiceCollection()
               .Configure<ServerOptions>(options =>
               {
                   options.Hostname4 = ServerOptions.Hostname4;
                   options.Port = ServerOptions.Port;
                   options.BindAddress4 = ServerOptions.BindAddress4;
               })
               .AddYllibedHttpServer()
               .AddOAuthCallbackHandlerAndRegister(configure =>
               {
                   configure.CallbackUri = CallbackHandlerOptions.CallbackUri;
               })
               .BuildServiceProvider();

            var server = serviceProvider.GetRequiredService<IHttpServer>();

            (_serverRootUri, _) = server.Start();

            var handler = serviceProvider.GetRequiredService<IAuthCallbackHandler>();
            return (_serverRootUri, handler);
        }
        
        (_serverRootUri, _) = _server.Start();

        return (_serverRootUri, _callbackHandler);
    }

    public Uri GetCurrentApplicationCallbackUri()
    {
        return new Uri(EnsureServerStarted().RootUri, "/callback");
    }
   
    /// <remarks>
    /// <paramref name="ct"/> not gets used by now.
    /// </remarks>
    public async Task<WebAuthenticationResult> AuthenticateAsync(
        WebAuthenticationOptions options,
        Uri requestUri,
        Uri callbackUri,
        CancellationToken ct)
    {

        CheckWebAuthOptionFlag(options);

        if (callbackUri is not null)
        {
            if (callbackUri.IsAbsoluteUri
                && callbackUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                && callbackUri.HostNameType is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6)
            {
                ServerOptions.Hostname4 = callbackUri.Host;
                ServerOptions.Port = callbackUri.IsDefaultPort ? ServerOptions.Port : (ushort)callbackUri.Port;
                ServerOptions.BindAddress4 = IPAddress.Loopback;

                CallbackHandlerOptions.CallbackUri = callbackUri.OriginalString;
            }
        }

        // ensure server is started
        var (rootUri, authCallbackHandler) = EnsureServerStarted();

        // open system browser
        BrowserProvider.OpenBrowser(requestUri);// BUG: no process started

        return await authCallbackHandler.WaitForCallbackAsync();

    }

    private void CheckWebAuthOptionFlag(WebAuthenticationOptions options)
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
    }

    
}
