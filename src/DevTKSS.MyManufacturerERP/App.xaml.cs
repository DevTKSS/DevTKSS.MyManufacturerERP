using Serilog;
using DevTKSS.MyManufacturerERP.Infrastructure.Defaults;
using DevTKSS.MyManufacturerERP.Infrastructure.Services;
using Microsoft.Extensions.Diagnostics.Metrics;


// this is somehow not detected even while its the correct namespace
//#if !WINDOWS
//using IWebAuthenticationBrokerProvider = Uno.AuthenticationBroker.IWebAuthenticationBrokerProvider;
//using Temp.Extensibility.DesktopAuthBroker;
//#endif

namespace DevTKSS.MyManufacturerERP;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = /*await*/ this.CreateBuilder(args)

          // Add navigation support for toolkit controls such as TabBar and NavigationView

          .UseToolkitNavigation()
          .Configure(host => host
#if DEBUG
             // Switch to Development environment when running in DEBUG
             .UseEnvironment(Environments.Development)
#endif

             .UseLogging(configure: (context, logBuilder) =>
             {
                 // Configure log levels for different categories of logging
                 logBuilder
                     .SetMinimumLevel(
                         context.HostingEnvironment.IsDevelopment() ?
                             LogLevel.Information :
                             LogLevel.Warning)

                     // Default filters for core Uno Platform namespaces
                     .CoreLogLevel(LogLevel.Warning);

                 // Uno Platform namespace filter groups
                 // Uncomment individual methods to see more detailed logging
                 //// Generic Xaml events
                 //logBuilder.XamlLogLevel(LogLevel.Debug);
                 //// Layout specific messages
                 //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                 //// Storage messages
                 //logBuilder.StorageLogLevel(LogLevel.Debug);
                 //// Binding related messages
                 //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                 //// Binder memory references tracking
                 //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                 //// DevServer and HotReload related
                 //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                 //// Debug JS interop
                 //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

             }, enableUnoLogging: true)
             .UseSerilog(consoleLoggingEnabled: true, fileLoggingEnabled: true)
             .UseConfiguration(configure: unoConfigBuilder =>
                 unoConfigBuilder
                    .EmbeddedSource<App>()
                    .Section<AppConfig>()
                    .Section<OAuthOptions>("EtsyOAuthOptions")
                    .Section<ApiKeyOptions>("SevdeskApiKeyClient")
             )
            // Enable localization (see appsettings.json for supported languages)
            .UseLocalization()
            // Register Json serializers (ISerializer and ISerializer)
            .UseSerialization((context, services) => services
                // Adding String TypeInfo <see href="https://github.com/unoplatform/uno/issues/20546"/>
                .AddContentSerializer(context)
                .AddJsonTypeInfo(OAuthFlowContext.Default.AccessGrantResponse)
                .AddJsonTypeInfo(OAuthFlowContext.Default.TokenResponse)
                .AddJsonTypeInfo(EtsyJsonContext.Default.OAuthOptions)
                .AddJsonTypeInfo(EtsyJsonContext.Default.UserDetailsResponse)
                .AddJsonTypeInfo(EtsyJsonContext.Default.UserMeResponse)
                .AddJsonTypeInfo(EtsyJsonContext.Default.PingResponse)
            )
            .UseHttp((context, services) =>
            {
#if DEBUG
                // DelegatingHandler will be automatically injected
                services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                services.AddRefitClientWithEndpoint<IEtsyOAuthEndpoints, OAuthOptions>(
                    context: context,
                    options: context.Configuration.GetSection("EtsyOAuthOptions").Get<OAuthOptions>(),
                    name: "EtsyClient",
                    configure: (clientBuilder, options) => clientBuilder
                    .ConfigureHttpClient(httpClient =>
                    {
                        httpClient.BaseAddress = new Uri("https://openapi.etsy.com");
                        if (options?.ClientID is null)
                        {
                            throw new ArgumentNullException(options?.ClientID,nameof(options.ClientID)/*, "API Key must be provided in the configuration."*/);
                        }
                        httpClient.DefaultRequestHeaders.Add("x-api-key", options.ClientID);
                    })
                )
                .AddRefitClient<IEtsyUserEndpoints>(context);
            })
            .UseAuthentication(authBuilder =>
                authBuilder.AddWeb<EtsyOAuthAuthenticationDelegate>(name: "EtsyOAuth"),
                configureAuthorization: builder =>
                {
                    builder.AuthorizationHeader(scheme: "Bearer");
                }
            )

            .ConfigureServices((context, services) =>
            {
                // TODO: Register your sp
//#if !WINDOWS
//                services.AddSingleton<IWebAuthenticationBrokerProvider, SystemBrowserAuthBroker>();
//#endif

            })
            .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
        );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>
        (initialNavigate: async (services, navigator) =>
        {
            var auth = services.GetRequiredService<IAuthenticationService>();
            var authenticated = await auth.RefreshAsync();
            if (authenticated)
            {
                await navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.Nested);
            }
            else
            {
                await navigator.NavigateViewModelAsync<LoginModel>(this, qualifier: Qualifiers.Nested);
            }
        });
    }

    private async ValueTask<IDictionary<string, string>?> HandleRefresh(IDictionary<string, string> tokenDictionary)
    {
        // TODO: Write code to refresh tokens using the currently stored tokens
        if ((tokenDictionary?.TryGetValue(OAuthTokenRefreshDefaults.RefreshToken, out var refreshToken) ?? false) && !refreshToken.IsNullOrEmpty()
         && (tokenDictionary?.TryGetValue(OAuthTokenRefreshDefaults.ExpiresInKey, out var expiry) ?? false) && DateTime.TryParse(expiry, out var tokenExpiry) && tokenExpiry > DateTime.Now)
        {
            // Return IDictionary containing any tokens used by service calls or in the app
            tokenDictionary ??= new Dictionary<string, string>();
            tokenDictionary[OAuthTokenRefreshDefaults.AccessTokenKey] = "NewSampleToken";
            tokenDictionary[OAuthTokenRefreshDefaults.ExpiresInKey] = DateTime.Now.AddMinutes(5).ToString("g");
            return tokenDictionary;
        }

        // Return null/default to fail the Refresh method
        return default;
    }

    private async ValueTask<IDictionary<string, string>?> ProcessCredentials(IDictionary<string, string> credentials)
    {
        // TODO: Write code to process credentials that are passed into the LoginAsync method
        if (credentials?.TryGetValue(nameof(LoginModel.Username), out var username) ?? false && !username.IsNullOrEmpty())
        {
            // Return IDictionary containing any tokens used by service calls or in the app
            credentials ??= new Dictionary<string, string>();
            credentials[OAuthTokenRefreshDefaults.AccessTokenKey] = "SampleToken";
            credentials[OAuthTokenRefreshDefaults.RefreshToken] = "RefreshToken";
            credentials[OAuthTokenRefreshDefaults.ExpiresInKey] = DateTime.Now.AddMinutes(5).ToString("g");
            return credentials;
        }

        // Return null/default to fail the LoginAsync method
        return default;
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<LoginPage, LoginModel>(),
            new ViewMap<MainPage, MainModel>(),
            new DataViewMap<SecondPage, SecondModel, Entity>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new ("Login", View: views.FindByViewModel<LoginModel>()),
                    new ("Main", View: views.FindByViewModel<MainModel>(), IsDefault:true),
                    new ("Second", View: views.FindByViewModel<SecondModel>()),
                ]
            )
        );
    }
}
//(IServiceProvider services, ITokenCache tokens, IDictionary<string, string>? authenticationTokens, string? baseurl, string callbackUrl) =>
//                    {
//                        if (authenticationTokens is null)
//                        {
//                            throw new ArgumentNullException(nameof(authenticationTokens), "Authentication tokens cannot be null.");
//                        }
//                        string scopes = authBuilder.Get<OAuthOptions>("Etsy")?.Scopes;
//scopes = HttpUtility.UrlEncode(scopes);
//return new Uri(baseurl, $"?client_id={authenticationTokens["ClientID"]}&response_type=code&scope={scopes}&redirect_uri={services.LoginCallbackUri}");

//                    });