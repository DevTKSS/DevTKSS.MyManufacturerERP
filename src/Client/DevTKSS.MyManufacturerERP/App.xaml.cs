using DesktopAuthenticationIntegration;
using Uno.AuthenticationBroker;
using DevTKSS.Extensions.OAuth;

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

    public Window? MainWindow { get; private set; }
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
             .UseConfiguration(configure: unoConfigBuilder =>
                 unoConfigBuilder
                    .EmbeddedSource<App>()
                    .Section<AppConfig>()
                    .Section<EtsyOAuthEndpointOptions>(EtsyOAuthEndpointOptions.SectionName)
             )
             .UseLogging(configure: (context, logBuilder) =>
             {
                 // Configure log levels for different categories of logging
                 logBuilder
                     .SetMinimumLevel(
                         context.HostingEnvironment.IsDevelopment() ?
                             LogLevel.Trace :
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
            .UseValidation(configure:(validatorBuilder) => validatorBuilder
                .Validator<OAuthEndpointOptions,OAuthEndpointOptionsValidator>())
            
            // Enable localization (see appsettings.json for supported languages)
            .UseLocalization()
            .ConfigureServices((context, services) =>
            {
                services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("DevTKSS.Extensions.") ?? false));

                // Register WebAPI OAuth client (Refit) - replaces direct Etsy integration
                services.AddRefitClient<IWebApiOAuthEndpoints>()
                    .ConfigureHttpClient(httpClient =>
                    {
                        // Point to local WebAPI
                        #if DEBUG
                        httpClient.BaseAddress = new Uri("http://localhost:5000");
                        #else
                        httpClient.BaseAddress = new Uri(builder.Configuration["WebApi:BaseUrl"] ?? "https://api.example.com");
                        #endif
                    });
            })
            .UseHttp((context, services) =>
            {
#if DEBUG
                // DelegatingHandler will be automatically injected
                services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
            })

            .UseAuthentication(authBuilder =>
            {
                authBuilder.AddCustom(authBuilder =>
                {
                    authBuilder.Login(HandleLoginAsync);
                    authBuilder.Refresh(HandleRefreshAsync);
                    authBuilder.Logout(HandleLogoutAsync);
                });
                #region Web Auth configuration
                // reference used: https://github.com/unoplatform/uno.extensions/blob/main/testing/TestHarness/TestHarness/Ext/Authentication/Web/WebAuthenticationHostInit.cs
                //authBuilder.AddWeb<IEtsyOAuthEndpoints>(configureWeb =>
                //configureWeb
                //    .AccessTokenKey(OAuthTokenRefreshDefaults.AccessTokenKey)
                //    .RefreshTokenKey(OAuthTokenRefreshDefaults.RefreshToken)
                //    .PrepareLoginCallbackUri(
                //        async(service,serviceProvider,tokencache,loginCallbackUri,ct)
                //        => loginCallbackUri!)

                //    .PrepareLoginStartUri(async (sp, tokens, credentials, loginStartUri, ct)
                //        => await CreateLoginStartUri(sp, tokens, credentials, loginStartUri, ct))

                //    .PostLogin(async(authService, serviceProvider,tokenCache, credentials, redirectUri, tokens,cancellationToken)
                //        => await ProcessPostLoginAsync(authService, serviceProvider,tokenCache,credentials,redirectUri,tokens, cancellationToken))

                //    .Refresh(async (authService, serviceProvider, tokenCache, tokens, cancellationToken) =>
                //        await RefreshTokensAsync(authService, serviceProvider, tokenCache, tokens, cancellationToken))

                //    ,name: "EtsyOAuth"),
                //},
                //    configureAuthorization: builder =>
                //    {
                //        builder.AuthorizationHeader(scheme: "Bearer");
                //    }
                #endregion
            },configureAuth =>
                configureAuth.AuthorizationHeader(scheme: "Bearer")
            )
            .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
        );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
    //    MainWindow.SetWindowIcon();

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
                await navigator.NavigateViewModelAsync<AuthModel>(this, qualifier: Qualifiers.Nested);
            }
        });
    }

    private async ValueTask<IDictionary<string, string>?> HandleLoginAsync(IServiceProvider serviceProvider, IDispatcher? dispatcher, IDictionary<string,string> credentials,  CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("OAuth Login via WebAPI");

        try
        {
            var oauthClient = serviceProvider.GetRequiredService<IWebApiOAuthEndpoints>();
            
            // Call WebAPI /auth/login endpoint
            // WebAPI will handle Etsy OAuth flow and set authentication cookie
            var response = await oauthClient.LoginAsync(ct);
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("WebAPI login failed with status {Status}", response.StatusCode);
                return null;
            }

            logger.LogInformation("OAuth login successful");
            
            // Return tokens to Uno authentication system
            return new Dictionary<string, string>
            {
                { "access_token", "authenticated-via-webapi" },
                { "token_type", "Bearer" }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OAuth login failed");
            return null;
        }
    }

    private async ValueTask<IDictionary<string, string>?> HandleRefreshAsync(IServiceProvider serviceProvider, ITokenCache tokenCache, IDictionary<string, string> tokens, CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Token refresh via WebAPI");

        try
        {
            var oauthClient = serviceProvider.GetRequiredService<IWebApiOAuthEndpoints>();
            
            // Call WebAPI /auth/profile to verify current authentication
            var profile = await oauthClient.GetProfileAsync(ct);
            
            logger.LogInformation("Token refresh successful - User: {UserId}", profile.UserId);
            
            // Tokens are still valid, return them
            return tokens;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("Authentication expired, user needs to login again");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token refresh failed");
            return tokens; // Return existing tokens on error
        }
    }

    private async ValueTask<bool> HandleLogoutAsync(IServiceProvider ServiceProvider, IDispatcher? dispatcher, ITokenCache tokenCache, IDictionary<string,string> tokens, CancellationToken ct)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Logout via WebAPI");
        
        try
        {
            var oauthClient = ServiceProvider.GetRequiredService<IWebApiOAuthEndpoints>();
            
            // Call WebAPI /auth/logout endpoint
            var response = await oauthClient.LogoutAsync(ct);
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("WebAPI logout failed with status {Status}", response.StatusCode);
            }
            else
            {
                logger.LogInformation("WebAPI logout successful");
            }
            
            // Clear local token cache regardless of WebAPI response
            await tokenCache.ClearAsync(ct);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Logout error");
            // Clear tokens anyway
            await tokenCache.ClearAsync(ct);
            return true;
        }
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<AuthPage, AuthModel>(),
            new ViewMap<MainPage, MainModel>(),
            new DataViewMap<SecondPage, SecondModel, Entity>(),
            new ViewMap<WebViewBrowserPage, WebViewBrowserModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    
                    new ("Main", View: views.FindByViewModel<MainModel>(), IsDefault:true),
                    new ("Second", View: views.FindByViewModel<SecondModel>()),
                    new ("Auth", View: views.FindByViewModel<AuthModel>()),
                    new ("WebViewRouter", View: views.FindByViewModel<WebViewBrowserModel>())
                ]
            )
        );
    }
}
