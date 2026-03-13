namespace DevTKSS.MyManufacturerERP;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
#pragma warning disable IDE0003 // Remove qualification
        this.InitializeComponent();
#pragma warning restore IDE0003 // Remove qualification
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
              configure: unoConfigBuilder =>
                 unoConfigBuilder
                    .EmbeddedSource<App>()
                    .Section<AppConfig>()
                    // Note: "Web" section is loaded automatically by Web Authentication Providers
                    .Section<EtsyOAuthEndpointOptions>(EtsyOAuthEndpointOptions.SectionName)
                    .Section<OAuthClientOptions>(OAuthClientOptions.SectionName)
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
                .Validator<OAuthClientOptions, OAuthClientOptionsValidator>()
                .Validator<OAuthOptions,OAuthOptionsValidator>())

            // Enable localization (see appsettings.json for supported languages)
            .UseLocalization()
            .ConfigureServices((context, services) =>
            {
                services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("DevTKSS.Extensions.") ?? false));

                services.AddHttpClient<IOAuthTokenClient, OAuthTokenHttpClient>();
#if DESKTOP
                services.AddSingleton<IOAuthNavigationService, OAuthNavigationService>();
#endif
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
                    customBuilder.Login(HandleLoginCallbackAsync);
                    customBuilder.Refresh(HandleRefreshAsync);
                    customBuilder.Logout(HandleLogoutAsync);
                }, name: "Custom");
#elif BROWSERWASM
                // Web Authentication Providers for WebAssembly
                authBuilder.AddWeb(configure: configureWeb =>
                    configureWeb
                    .AccessTokenKey(OAuthDefaults.Keys.AccessToken)
                    .RefreshTokenKey(OAuthDefaults.Keys.RefreshToken)
                    .PrepareLoginCallbackUri(async (service, serviceProvider, tokenCache, loginCallbackUri, ct) =>
                    {
                        // Use WebAssembly-specific callback URI // Ignore async, no await, NO Task.FromResult!!!
                        return new Uri("http://localhost:3000/auth/callback").OriginalString;
                    })
                    .PrepareLoginStartUri(async (sp, credentials, ct) =>
                    {
                        // Use configured login URI from appsettings
                        var redirectUri = new Uri(sp.GetRequiredService<IConfiguration>()
                                         .GetSection("Web")
                                         .GetValue<string>("LoginStartUri") ?? "http://localhost:5000/auth/login").OriginalString;
                        var options = sp.GetRequiredService<IOptions<OAuthClientOptions>>().Value;
                        if (options is not { ClientId: not null, RedirectUri: not null, Scopes: not null and { Length: > 0 } })
                        {
                            throw new InvalidOperationException("OAuth ClientId, RedirectUri, or Scopes not configured");
                        }
                        var state = OAuth2Utilitys.GenerateState();
                        var codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
                        var challenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier);

                        credentials ??= new Dictionary<string, string>();
                        credentials.AddOrReplace(OAuthDefaults.Keys.State, state);
                        credentials.AddOrReplace(OAuthDefaults.Keys.Pkce.CodeVerifier, codeVerifier);
                        var authRequest = new AuthorizationCodeRequest()
                        {
                            ClientId = options.ClientId!,
                            RedirectUri = options.RedirectUri!,
                            Scope = options.Scopes.JoinBy(" "),
                            State = state,
                            CodeChallenge = challenge,
                        }.ToDictionary();

                        return new UriBuilder(redirectUri).AppendQueryParameters(authRequest).Uri.OriginalString;

                    })
                    .PostLogin(async (serviceProvider, tokenCache, credentials, tokens, cancellationToken) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<IOAuthTokenClient>>();
                        // TODO: Implement processing of redirect URI to extract tokens
                        credentials?.Clear();
                        logger.LogInformation("Web authentication completed successfully");
                        return tokens;
                    })
                    .Refresh(async (serviceProvider, tokenCache, tokens, ct) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<IOAuthTokenClient>>();
                        logger.LogInformation("Refreshing tokens via Web");
                        var options = serviceProvider.GetRequiredService<IOptions<OAuthClientOptions>>().Value;
                        if (options.ClientId is not { } clientId)
                        {
                            logger.LogError("OAuth ClientId is not configured, cannot refresh tokens");
                            throw new InvalidOperationException("OAuth ClientId is not configured");
                        }

                        var oauthClient = serviceProvider.GetRequiredService<IOAuthTokenClient>();
                        try
                        {
                            if (!tokens.TryGetRefreshToken(out var rt) || string.IsNullOrWhiteSpace(rt))
                            {
                                logger.LogWarning("No refresh token available");
                                return null;
                            }

                            var tokenResponse = await oauthClient.RefreshTokenAsync(new RefreshTokenRequest
                            {
                                ClientId = clientId,
                                RefreshToken = rt,
                            }, ct);

                            if (tokenResponse is not TokenResponse { AccessToken: not null, RefreshToken: not null, ExpiresIn: > 0, TokenType: OAuthDefaults.Values.Bearer } response)
                            {
                                logger.LogError("Token refresh response missing required tokens");
                                return null;
                            }

                            tokens.AddOrReplace(response.ToDictionary(false));

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
                    //    .PrepareLoginCallbackUriAsync(
                    //        async(service,serviceProvider,tokencache,loginCallbackUri,ct)
                    //        => loginCallbackUri!)

                    //    .PrepareLoginStartUriAsync(async (sp, tokens, credentials, loginStartUri, ct)
                    //        => await CreateLoginStartUri(sp, tokens, credentials, loginStartUri, ct))

                    //    .PostLoginAsync(async(authService, serviceProvider,tokenCache, credentials, redirectUri, tokens,cancellationToken)
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

    private static async ValueTask<IDictionary<string, string>?> HandleLoginCallbackAsync(IServiceProvider serviceProvider, IDispatcher? dispatcher, IDictionary<string, string> credentials, CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IOAuthTokenClient>>();
        logger.LogInformation("Starting OAuth login flow");

        try
        {
            var oauthClient = serviceProvider.GetRequiredService<IOAuthTokenClient>();
            var options = serviceProvider.GetRequiredService<IOptions<OAuthClientOptions>>().Value;
            logger.LogInformation("Calling authorization endpoint to initiate OAuth flow");
            
            if (options.ClientId is not { } clientId)
            {
                logger.LogError("OAuth ClientId is not configured, cannot refresh tokens");
                throw new InvalidOperationException("OAuth ClientId is not configured");
            }

            var state = OAuth2Utilitys.GenerateState();
            var codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
            var challenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier);

            //var tokenResponse = await oauthClient.ExchangeCodeAsync(new AccessTokenRequest
            //{
            //    ClientId = clientId,
            //    RedirectUri = options.RedirectUri!,
            //    Code = authorizationCode,
            //    CodeVerifier = codeVerifier,
            //}, ct);

            //if (tokenResponse is not TokenResponse { AccessToken: not null, RefreshToken: not null, ExpiresIn: > 0, TokenType: OAuthDefaults.Values.Bearer } response)
            //{
            //    logger.LogError("Token exchange response missing required tokens");
            //    return default;
            //}

            // TODO: Extract authorization code from redirect URI

            // TODO: Validate state and code returned from OAuth provider

            // TODO: Exchange authorization code for access and refresh tokens

            // TODO: Depending on the OAuth provider, extract IdToken from Response/possible needs to be parsed. See EtsyOAuthProvider using UserID

            // TODO: If provided, loop through tokenOptions.AdditionalTokenKeys to extract additional tokens

            // TODO: Return tokens which will be automatically stored internally from Uno.Extensions.Authentication ITokenCache
           // return response.ToDictionary(false);
           return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OAuth login flow failed with exception");
            return null;
        }
    }

    private static async ValueTask<IDictionary<string, string>?> HandleRefreshAsync(IServiceProvider serviceProvider, ITokenCache tokenCache, IDictionary<string, string> tokens, CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IOAuthTokenClient>>();
        var options = serviceProvider.GetRequiredService<IOptions<EtsyOAuthEndpointOptions>>().Value;
        var oauthClient = serviceProvider.GetRequiredService<IOAuthTokenClient>();
        logger.LogInformation("Token refresh flow started");
        if (!tokens.TryGetValue(options.TokenKeys.RefreshTokenKey, out var rt))
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
            var tokenResponse = await oauthClient.RefreshTokenAsync(new RefreshTokenRequest()
            {
                ClientId = clientId,
                RefreshToken = rt
            }, ct);

            if (tokenResponse is not TokenResponse { AccessToken: not null, RefreshToken: not null, ExpiresIn: > 0, TokenType: OAuthDefaults.Values.Bearer } response)
            {
                logger.LogError("Token refresh response missing required tokens");
                return default;
            }

            tokens[options.TokenKeys.AccessTokenKey] = response.AccessToken;
            tokens[options.TokenKeys.RefreshTokenKey] = response.RefreshToken;
            tokens[options.TokenKeys.ExpiresInKey] = DateTime.Now.AddSeconds(response.ExpiresIn).ToString("g");

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
            return tokens;
        }
    }

    private static async ValueTask<bool> HandleLogoutAsync(IServiceProvider ServiceProvider, IDispatcher? dispatcher, ITokenCache tokenCache, IDictionary<string,string> tokens, CancellationToken ct)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<IOAuthTokenClient>>();

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
