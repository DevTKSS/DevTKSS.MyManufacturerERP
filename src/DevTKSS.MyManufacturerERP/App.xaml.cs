
using Serilog;
#if WINDOWS
using Microsoft.Security.Authentication.OAuth;
#endif
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

        var builder = this.CreateBuilder(args) // add await if uncommenting OpenIddict
            
//        // https://github.com/Ecierge/openiddict-core/blob/uno/sandbox/OpenIddict.Sandbox.Uno.Client/App.xaml.cs
//            .UseOpenIddictClientActivationHandlingAsync((context, services) =>
//            {
//                services.AddDbContext<DbContext>(options =>
//                {
//                    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "devtkss-my-manufacturer-erp.sqlite3")}");
//                    options.UseOpenIddict();
//                });
//                services.AddOpenIddict()
//                    // Register the OpenIddict core components
//                    .AddCore(options =>
//                    {
//                        // Configure OpenIddict to use the Entity Framework Core stores
//                        options.UseEntityFrameworkCore()
//                            .UseDbContext<DbContext>();
//                    })
//                    // Register the OpenIddict client components.
//                    .AddClient(options =>
//                    {
//                        // Note: this sample uses the authorization code and refresh token
//                        // flows, but you can enable the other flows if necessary.
//                        options.AllowAuthorizationCodeFlow()
//                               .AllowRefreshTokenFlow();

//                        // Register the signing and encryption credentials used to protect
//                        // sensitive data like the state tokens produced by OpenIddict.
//                        options.AddDevelopmentEncryptionCertificate()
//                               .AddDevelopmentSigningCertificate();

//                        //options.UseSystemIntegration();
//                        options.UseUnoIntegration()
//#if ANDROID || IOS
//                            .DisableEmbeddedWebServer()
//                            .DisablePipeServer()
//#endif
//                        ;
//                        // Register the System.Net.Http integration and use the identity of the current
//                        // assembly as a more specific user agent, which can be useful when dealing with
//                        // providers that use the user agent as a way to throttle requests (e.g Reddit).
//                        options.UseSystemNetHttp()
//                               .SetProductInformation(typeof(App).Assembly);

//                        // Add a client registration matching the client application definition in the server project.
//                        options.AddRegistration(new OpenIddictClientRegistration
//                        {
//#if ANDROID
//                            Issuer = new Uri("https://10.0.2.2:44395/", UriKind.Absolute),
//#else
//                            Issuer = new Uri("https://localhost:5001/", UriKind.Absolute),
//#endif
//                            DefaultName = "Local",

//                            ClientId = "uno",

//                            // This sample uses protocol activations with a custom URI scheme to handle callbacks.
//                            //
//                            // For more information on how to construct private-use URI schemes,
//                            // read https://www.rfc-editor.org/rfc/rfc8252#section-7.1 and
//                            // https://www.rfc-editor.org/rfc/rfc7595#section-3.8.
//                            PostLogoutRedirectUri = new Uri("com.DevTKSS.MyManufacturerERP:/callback/logout/local", UriKind.Absolute),
//                            RedirectUri = new Uri("com.openiddict.sandbox.uno.client:/callback/login/local", UriKind.Absolute),

//                            Scopes = { Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, "demo_api" }
//                        });

//                        //// Register the Web providers integrations.
//                        ////
//                        //// Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
//                        //// address per provider, unless all the registered providers support returning an "iss"
//                        //// parameter containing their URL as part of authorization responses. For more information,
//                        //// see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
//                        //options.UseWebProviders()

//                        //       .AddGitHub(options =>
//                        //       {
//                        //           options.SetClientId("8abc54b6d5f4e39d78aa")
//                        //                  .SetClientSecret("f37ef38bdb18a0f5f2d430a8edbed4353c012dc3")
//                        //                  // Note: GitHub doesn't support the recommended ":/" syntax and requires using "://".
//                        //                  .SetRedirectUri("com.openiddict.sandbox.uno.client://callback/login/github");
//                        //       });
//                    });

//                // Register the worker responsible for creating the database used to store tokens
//                // and adding the registry entries required to register the custom URI scheme.
//                //
//                // Note: in a real world application, this step should be part of a setup script.
//                services.AddHostedService<Worker>();
//            },
//            "DevTKSS.MyManufacturerERP");
        
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
                            
                            .Section<OAuthConfiguration>() // This is the section name for the Etsy API configuration
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                // Register Json serializers (ISerializer and ISerializer)
                .UseSerialization((context, services) => services
                    // Adding String TypeInfo <see href="https://github.com/unoplatform/uno/issues/20546"/>
                    .AddContentSerializer(context)
                    .AddJsonTypeInfo(EtsyJsonContext.Default.TokenRequestPayload)
                    .AddJsonTypeInfo(EtsyJsonContext.Default.TokenResponse)
                    .AddJsonTypeInfo(EtsyJsonContext.Default.EtsyOAuthOptions)
                )
                .UseHttp((context, services) =>
                {
#if DEBUG
                    // DelegatingHandler will be automatically injected
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                    services.AddRefitClientWithEndpoint<IEtsyUserEndpoints, OAuthConfiguration>(
                       context,
                       name:"EtsyClient",
                       configure: (clientBuilder, options) => clientBuilder
                       .ConfigureHttpClient(httpClient =>
                       {
                           httpClient.BaseAddress = new Uri(options?.Url ?? throw new ArgumentNullException(nameof(OAuthConfiguration.Url)));
                           httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                       }))
                       ;//.AddRefitClient<IEtsyUserEndpoints>(context);


                    // services.AddKiotaClient<MyManufacturerERPApiClient>(context);
                })
                //.UseAuthentication(authBuilder =>

                // //{
                // //    // Configure authentication services
                // //    // Refering to the docs: <see href="https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#3-configure-the-provider"/>
                // //    // The Auth Provider is named equal to the appsettings section name
                // //    // BUG: Clicking on the "Login" button on the LoginPage opens a Browser (correct) but seems like the server never gets started
                // //    // Setting the port in launchSettings.json to 5000 and server to 5001+5002 does fail to build
                // //    // Setting the port of WebAssembly Target to 5001+5002 also, gets stuck at the SplashScreen
                // //    authBuilder.AddWeb(name: "WebAuth")//,configure:webAuthBuilder =>
                // //    {
                // //        // Configure the PostLogin processor
                // //        // <see href="https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#4-process-post-login-tokens"/>
                // //        // But looking at the Type VS2022 IntelliSense shows, the first parameter is no AuthService, then a <see cref="IDictionary<string, string>"/>
                // //        // so I am not sure if this is correct, but it's what the Uno Docs saying
                // //        webAuthBuilder.PostLogin(async (authService, tokens, ct) =>
                // //        {
                // //            // Process the Response here
                // //            return tokens;
                // //        });
                // //    });

                // // No idea how to use this for e.g. oAuth2 as the IAuthenticationProvider interface does not match.
                // authBuilder.AddCustom(customAuth =>
                //     customAuth.Login(
                //         async (sp, dispatcher, credentials, cancellationToken) =>
                //         {
                //             var isValid = credentials.TryGetValue("Username", out var username) && username == "Bob";
                //             return isValid ? credentials : default;
                //         })
                //))
                .ConfigureServices((context, services) =>
                {
                    // TODO: Register your services

                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
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
