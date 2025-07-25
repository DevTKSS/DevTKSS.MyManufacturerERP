using Uno.Resizetizer;
using Serilog;
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
        //IDictionary<string,string> envVars = DotEnv.Fluent()
        //        .WithEnvFiles(".env")
        //        .WithoutTrimValues() // Important to keep this, 
        //        .WithExceptions()
        //        .WithOverwriteExistingVars()
        //        .WithProbeForEnv(5)
        //        .Read();

        var builder = this.CreateBuilder(args)
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
                .UseConfiguration(
                //configureHostConfiguration:configurationBuilder =>
                //{
                //    configurationBuilder.AddEnvironmentVariables();
                //},
                configure: unoConfigBuilder =>
                {
                    // Used encapusated write syntax to improve readability, its doing the same as without brackets + return
                    return unoConfigBuilder
                            .EmbeddedSource<App>()
                            .Section<AppConfig>();
                })
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                // Register Json serializers (ISerializer and ISerializer)
                .UseSerialization((context, services) => services
                    // Adding String TypeInfo <see href="https://github.com/unoplatform/uno/issues/20546"/>
                    .AddContentSerializer(context)
                    .AddJsonTypeInfo(WeatherForecastContext.Default.WeatherForecast)
                    .AddJsonTypeInfo(WeatherForecastContext.Default.IImmutableListWeatherForecast)
                    .AddJsonTypeInfo(TodoItemContext.Default.TodoItem)
                    .AddJsonTypeInfo(TodoItemContext.Default.IImmutableListTodoItem))
                .UseHttp((context, services) =>
                {
#if DEBUG
                    // DelegatingHandler will be automatically injected
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif

                    // services.AddKiotaClient<MyManufacturerERPApiClient>(context);
                })
                //.UseAuthentication(authBuilder =>
                //{
                //    // Configure authentication services
                //    // Refering to the docs: <see href="https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#3-configure-the-provider"/>
                //    // The Auth Provider is named equal to the appsettings section name
                //    // BUG: Clicking on the "Login" button on the LoginPage opens a Browser (correct) but seems like the server never gets started
                //    // Setting the port in launchSettings.json to 5000 and server to 5001+5002 does fail to build
                //    // Setting the port of WebAssembly Target to 5001+5002 also, gets stuck at the SplashScreen
                //    authBuilder.AddWeb(name: "WebAuth")//,configure:webAuthBuilder =>
                //    {
                //        // Configure the PostLogin processor
                //        // <see href="https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#4-process-post-login-tokens"/>
                //        // But looking at the Type VS2022 IntelliSense shows, the first parameter is no AuthService, then a <see cref="IDictionary<string, string>"/>
                //        // so I am not sure if this is correct, but it's what the Uno Docs saying
                //        webAuthBuilder.PostLogin(async (authService, tokens, ct) =>
                //        {
                //            // Process the Response here
                //            return tokens;
                //        });
                //    });
                // No idea how to use this for e.g. oAuth2 so I will not use it for now
                // authBuilder.AddCustom(customAuth =>
                //     customAuth.Login(
                //         async (sp, dispatcher, credentials, cancellationToken) =>
                //         {
                //             var isValid = credentials.TryGetValue("Username", out var username) && username == "Bob";
                //             return isValid ? credentials : default;
                //         })
                //);
                //},
                //configureAuthorization: configure =>
                //{
                //    // Configure Cookies using the Uno Docs
                //    // <see href="https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-Cookies.html"/>
                //    // Absolutly no Idea how to use this, but it is in the Uno docs
                //    // I just would like to get Cookie Authentication to my Server Project API
                //    // Are there any examples? How can or should we name the Tokens below? Conventions?
                //    configure.Cookies("AccessToken", "RefreshToken");
                    // configure.AuthorizationHeader("Bearer"); }
              // )
                .ConfigureServices((context, services) =>
                {
                    // TODO: Register your services
                    //services.AddSingleton<IMyService, MyService>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
            //(initialNavigate: async (services, navigator) =>
            //{
            //    var auth = services.GetRequiredService<IAuthenticationService>();
            //    var authenticated = await auth.RefreshAsync();
            //    if (authenticated)
            //    {
            //        await navigator.NavigateViewModelAsync<MainModel>(this, qualifier: Qualifiers.Nested);
            //    }
            //    else
            //    {
            //        await navigator.NavigateViewModelAsync<LoginModel>(this, qualifier: Qualifiers.Nested);
            //    }
            //});
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
