

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
             .UseConfiguration(
              configureAppConfiguration: (context, configBuilder) =>
              {
                  configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                  configBuilder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
              },
              configure: unoConfigBuilder =>
                 unoConfigBuilder
                    .EmbeddedSource<App>()
                    .Section<AppConfig>()
                    // Note: "Web" section is loaded automatically by Web Authentication Provider
                    .Section<EtsyOAuthEndpointOptions>(EtsyOAuthEndpointOptions.SectionName)
                    .Section<OAuthEndpointOptions>(OAuthEndpointOptions.SectionName)
                    .Section<ServerOptions>()
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
            .UseValidation(configure: (validatorBuilder) => validatorBuilder
                .Validator<OAuthEndpointOptions, OAuthEndpointOptionsValidator>())

            // Enable localization (see appsettings.json for supported languages)
            .UseLocalization()
            .ConfigureServices((context, services) =>
            {
                services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("DevTKSS.Extensions.") ?? false));

                // Register WebAPI OAuth client (Refit) - replaces direct Etsy integration
                services.AddRefitClient<IOAuthEndpoints>()
                    .ConfigureHttpClient(httpClient =>
                    {
                        httpClient.BaseAddress = new Uri("https://openapi.etsy.com/v3");
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
#if DESKTOP
                authBuilder.AddCustom(customBuilder =>
                {
                    customBuilder.Login(HandleLoginAsync);
                    customBuilder.Refresh(HandleRefreshAsync);
                    customBuilder.Logout(HandleLogoutAsync);
                }, name: "Custom");
#elif BROWSERWASM
                // Web Authentication Provider for WebAssembly
                authBuilder.AddWeb(configure: configureWeb =>
                    configureWeb
                    .AccessTokenKey("access_token")
                    .RefreshTokenKey("refresh_token")
                    .PrepareLoginCallbackUri(async (service, serviceProvider, tokenCache, loginCallbackUri, ct) =>
                    {
                        // Use WebAssembly-specific callback URI
                        return await Task.FromResult(new Uri("http://localhost:3000/auth/callback").ToString());
                    })
                    .PrepareLoginStartUri(async (sp, credentials, ct) =>
                    {
                        // Use configured login URI from appsettings
                        return new Uri(sp.GetRequiredService<IConfiguration>().GetSection("Web").GetValue<string>("LoginStartUri") ?? "http://localhost:5000/auth/login").ToString();
                    })
                    .PostLogin(async (serviceProvider, tokenCache, possiblyNullCredentialsDictionary, tokensDictionary, cancellationToken) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<App>>();
                        logger.LogInformation("Web authentication completed successfully");
                        return tokensDictionary;
                    })
                    .Refresh(async (serviceProvider, tokenCache, tokens, ct) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<App>>();
                        logger.LogInformation("Refreshing tokens via Web");
                        var options = serviceProvider.GetRequiredService<IOptions<OAuthEndpointOptions>>().Value;
                        if (options.ClientId is not { } clientId)
                        {
                            logger.LogError("OAuth ClientId is not configured, cannot refresh tokens");
                            throw new InvalidOperationException("OAuth ClientId is not configured");
                        }
                        var tokenOptions = serviceProvider.GetRequiredService<IOptions<TokenKeyOptions>>()?.Value;
                        var oauthClient = serviceProvider.GetRequiredService<IOAuthEndpoints>();
                        try
                        {
                            var profile = await oauthClient.AuthenticateAsync(new AuthorizationCodeRequest()
                            {
                                ResponseType = OAuthAuthorizationCodeReqestDefaults.CodeKey,
                                ClientId = options.ClientId,
                                RedirectUri = options.RedirectUri!,
                                Scope = options.Scopes.JoinBy(" "),
                                State = _state,
                                CodeChallenge = _challenge,
                                CodeChallengeMethod = OAuthPkceDefaults.CodeChallengeMethodS256
                            },ct);
                            logger.LogInformation("Token refresh successful");
                            return tokens;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Token refresh failed");
                            return null;
                        }
                    })
              , name: "Web");
#endif
             }
             
#region Web Auth configuration (commented reference)
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
            , configure =>
                 configure
                    .Cookies(accessTokenCookie: "access_token", refreshTokenCookie: "refresh_token")
                    .AuthorizationHeader(scheme: "Bearer")
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
    string _state = string.Empty;
    string _challenge = string.Empty;
    private async ValueTask<IDictionary<string, string>?> HandleLoginAsync(IServiceProvider serviceProvider, IDispatcher? dispatcher, IDictionary<string, string> credentials, CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Starting OAuth login flow");

        try
        {
            var oauthClient = serviceProvider.GetRequiredService<IOAuthEndpoints>();
            var options = serviceProvider.GetRequiredService<IOptions<OAuthEndpointOptions>>().Value;
            var tokenOptions = serviceProvider.GetRequiredService<IOptions<TokenKeyOptions>>()?.Value;
            logger.LogInformation("Calling authorization endpoint to initiate OAuth flow");
            
            if (options.ClientId is not { } clientId)
            {
                logger.LogError("OAuth ClientId is not configured, cannot refresh tokens");
                throw new InvalidOperationException("OAuth ClientId is not configured");
            }

            _state = OAuth2Utilitys.GenerateCodeVerifier();
            _challenge = OAuth2Utilitys.GenerateCodeChallenge(_state);
            // Call WebAPI /auth/login endpoint
            // WebAPI will redirect to Etsy OAuth login page
            var response = await oauthClient.AuthenticateAsync(new AuthorizationCodeRequest()
            {
                ResponseType = OAuthDefaults.Values.Code,
                ClientId = options.ClientId,
                RedirectUri = options.RedirectUri!,
                Scope = options.Scopes.JoinBy(" "),
                State = _state,
                CodeChallenge = _challenge,
                CodeChallengeMethod = OAuthDefaults.Values.S256
            }, ct);

            logger.LogInformation("Opening OAuth login page: {Source}", options.AuthorizationEndpoint);

            // TODO: Open the OAuth login page for interactive oAuth flow and wait for the user to complete the OAuth flow
            // For Desktop (macOS/Linux): Opens system browser
            // For Desktop (Windows): Opens WebView2AuthenticationPage in Dialog or new Window
            // For Mobile (iOS/Android): Opens in-app browser
            // For WebAssembly: ???
           
            logger.LogInformation("Authentication response status: {Status}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("Authentication failed with status {Status}", response.StatusCode);
                }
                return default;
            }

            // TODO: Extract authorization code from redirect URI

            // TODO: Validate state and code returned from OAuth provider

            // TODO: Exchange authorization code for access and refresh tokens

            // TODO: Depending on the OAuth provider, extract IdToken from Response/possible needs to be parsed. See EtsyOAuthProvider using UserID

            // TODO: If provided, loop through tokenOptions.AdditionalTokenKeys to extract additional tokens

            // TODO: Return tokens which will be automatically stored internally from Uno.Extensions.Authentication ITokenCache
            return new Dictionary<string, string>
            {
                { tokenOptions?.AccessTokenKey ?? OAuthTokenRefreshDefaults.AccessTokenKey, accessToken },
                { tokenOptions?.RefreshTokenKey ?? OAuthTokenRefreshDefaults.RefreshToken, refreshToken },
                { OAuthTokenRefreshDefaults.ExpiresInKey, expiresInToken }

            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OAuth login flow failed with exception");
            return null;
        }
    }

    private async ValueTask<IDictionary<string, string>?> HandleRefreshAsync(IServiceProvider serviceProvider, ITokenCache tokenCache, IDictionary<string, string> tokens, CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IOAuthEndpoints>>();
        var options = serviceProvider.GetRequiredService<IOptions<EtsyOAuthEndpointOptions>>().Value;
        var tokenOptions = serviceProvider.GetRequiredService<IOptions<TokenKeyOptions>>()?.Value;
        var oauthClient = serviceProvider.GetRequiredService<IOAuthEndpoints>();
        logger.LogInformation("Token refresh flow started");
        if (!tokens.TryGetValue(tokenOptions?.RefreshTokenKey ?? OAuthDefaults.Keys.RefreshToken, out var rt))
        {
            logger.LogWarning("No refresh token available, user needs to login again");
            return default;
        }
        if (options.ClientId is not { } clientId)
        {
            logger.LogError("OAuth ClientId is not configured, cannot refresh tokens");
            throw new InvalidOperationException("OAuth ClientId is not configured");
        }
        try
        {

            // Call Refresh Token endpoint to verify current authentication
            var tokenResponse = await oauthClient.RefreshTokenAsync(refreshTokenRequest:
                new RefreshTokenRequest()
                {
                    GrantType = OAuthDefaults.Values.RefreshToken,
                    ClientId = options.ClientId,
                    RefreshToken = rt
                }, ct);

            // TODO: E.g. EtsyOAuthProvider uses user_id as IdToken which can be gotten from AccessToken/RefreshToken [user_id].[rest of AccessToken] or from /users/me endpoint by providing the AccessToken. /users/{user_id} instead returns human readable Id information like primary_email and first_name last_name
            var etsyClient = serviceProvider.GetRequiredService<IEtsyUserEndpoints>();
            var meResponse = await etsyClient.GetMeAsync(bearerToken: tokenResponse.RefreshToken!, apiKey: options.ClientId!, cancellationToken: ct);

            logger.LogInformation("Token refresh successful - User: '{UserId}' with ShopId: '{ShopId}'", meResponse.UserId, meResponse.ShopId);
            
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
        var logger = ServiceProvider.GetRequiredService<ILogger<IOAuthEndpoints>>();
        var oauthClient = ServiceProvider.GetRequiredService<IOAuthEndpoints>();

        logger.LogInformation("Logout starting");
        
        try
        {
            // Call logout endpoint if provider has one
            //var response = await oauthClient.LogoutAsync(ct);
            
            //if (!response.IsSuccessStatusCode)
            //{
            //    logger.LogWarning("logout failed with status {Status}", response.StatusCode);
            //}
            //else
            //{
            //    logger.LogInformation("logout successful");
            //}
            
            // Clear local token cache regardless of response
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
            new ViewMap<AuthenticationDialog, AuthDialogModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    
                    new ("Main", View: views.FindByViewModel<MainModel>(), IsDefault:true),
                    new ("Second", View: views.FindByViewModel<SecondModel>()),
                    new ("Auth", View: views.FindByViewModel<AuthModel>()),
                    new ("AuthDialog", View: views.FindByViewModel<AuthModel>())
                ]
            )
        );
    }
}
