using Uno.AuthenticationBroker;
using Uno.Foundation.Extensibility;
using DesktopAuthenticationIntegration;

// [assembly: ApiExtension(typeof(WebAuthenticationBrokerProvider), typeof(SystemBrowserAuthBroker))]

namespace DesktopAuthenticationIntegration;
public sealed class SystemBrowserAuthBroker()
    : ISystemBrowserAuthBrokerProvider // IWebAuthenticationBrokerProvider
{

    private IServiceProvider? serviceProvider;
    /// <summary>
    /// Gets or initializes the used server options.<br/>
    /// Defaults to http://localhost:5001 binding to loopback address.
    /// </summary>
    /// <value>The server options.</value>
    /// <remarks>
    /// <see cref="ServerOptions"/> can not be configured to use HTTPS because of <see cref="Yllibed.HttpServer.Server"/> uses TCPListener which does not support HTTPS natively.
    /// </remarks>
    public ServerOptions ServerOptions { get; private set; } = new ServerOptions()
    {
        Hostname4 = "localhost",
        Port = 5001,
        BindAddress4 = IPAddress.Loopback
    };

    private Uri? _serverRootUri;

    private (Uri RootUri, IAuthCallbackHandler Handler) EnsureServerStarted(string? callbackUri = null)
    {
        serviceProvider = new ServiceCollection()
            .Configure<ServerOptions>(options =>
            {
                options.Hostname4 = ServerOptions.Hostname4;
                options.Port = ServerOptions.Port;
                options.BindAddress4 = ServerOptions.BindAddress4;
            }) 
            .AddYllibedHttpServer(options =>
             {
                 options.Hostname4 = "localhost";
                 options.Port = 5001;
                 options.BindAddress4 = IPAddress.Loopback;
             }).AddOAuthCallbackHandlerAndRegister(configure =>
                configure.CallbackUri = ServerOptions.ToUri4(callbackUri ?? "/callback")?.ToString()
             ).BuildServiceProvider();

        var server = serviceProvider.GetRequiredService<Server>();
        
        if (_serverRootUri is null)
        {
            (_serverRootUri, _) = server.Start();
        }
       
        var handler = serviceProvider.GetRequiredService<OAuthCallbackHandler>();

        return (_serverRootUri, handler);
        
    }

    public Uri GetCurrentApplicationCallbackUri()
    {
        return new Uri(EnsureServerStarted().RootUri, "/callback");
    }

    public async Task<WebAuthenticationResult> AuthenticateAsync(
        WebAuthenticationOptions options,
        Uri requestUri,
        Uri callbackUri,
        CancellationToken ct)
    {

        CheckWebAuthOptionFlag(options);

        // ensure server is started
        var (rootUri, authCallbackHandler) = EnsureServerStarted();

        // open system browser
        // prefer injected browser provider when available
        BrowserProvider.OpenBrowser(requestUri);

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
