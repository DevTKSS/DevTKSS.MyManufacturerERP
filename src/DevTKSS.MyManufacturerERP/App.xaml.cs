// this is somehow not detected even while its the correct namespace
//#if !WINDOWS
//using IWebAuthenticationBrokerProvider = Uno.AuthenticationBroker.IWebAuthenticationBrokerProvider;
//using Temp.Extensibility.DesktopAuthBroker;
//#endif

using System.Text.RegularExpressions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Client;
using OpenIddict.Sandbox.UnoClient;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
         var builder = await this.CreateBuilder(args)
            .UseOpenIddictClientActivationHandlingAsync((context,services) =>
            {
                services.AddDbContext<DbContext>(options =>
                {
                    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-sandbox-uno-client.sqlite3")}");
                    options.UseOpenIddict();
                });

                services.AddOpenIddict()

                    // Register the OpenIddict core components.
                    .AddCore(options =>
                    {
                        // Configure OpenIddict to use the Entity Framework Core stores and models.
                        // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                        options.UseEntityFrameworkCore()
                               .UseDbContext<DbContext>();
                    })

                    // Register the OpenIddict client components.
                    .AddClient(options =>
                    {
                        // Note: this sample uses the authorization code and refresh token
                        // flows, but you can enable the other flows if necessary.
                        options.AllowAuthorizationCodeFlow()
                               .AllowRefreshTokenFlow();

                        // Register the signing and encryption credentials used to protect
                        // sensitive data like the state tokens produced by OpenIddict.
                        options.AddDevelopmentEncryptionCertificate()
                               .AddDevelopmentSigningCertificate();

                        //options.UseSystemIntegration();
                        options.UseUnoIntegration()
#if ANDROID || IOS
                            .DisableEmbeddedWebServer()
                            .DisablePipeServer()
#endif
                        ;

                        // Register the System.Net.Http integration and use the identity of the current
                        // assembly as a more specific user agent, which can be useful when dealing with
                        // providers that use the user agent as a way to throttle requests (e.g Reddit).
                        options.UseSystemNetHttp()
                               .SetProductInformation(typeof(App).Assembly);

                        // Add a client registration matching the client application definition in the server project.
                        options.AddRegistration(new OpenIddictClientRegistration
                        {
#if ANDROID
                            Issuer = new Uri("https://10.0.2.2:5001/", UriKind.Absolute),
#else
                            Issuer = new Uri("https://localhost:5001/", UriKind.Absolute),
#endif
                            ProviderName = "Local",

                            ClientId = "uno",

                            // This sample uses protocol activations with a custom URI scheme to handle callbacks.
                            //
                            // For more information on how to construct private-use URI schemes,
                            // read https://www.rfc-editor.org/rfc/rfc8252#section-7.1 and
                            // https://www.rfc-editor.org/rfc/rfc7595#section-3.8.
                            PostLogoutRedirectUri = new Uri("com.openiddict.sandbox.uno.client:/callback/logout/local", UriKind.Absolute),
                            RedirectUri = new Uri("com.openiddict.sandbox.uno.client:/callback/login/local", UriKind.Absolute),

                            Scopes = { Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, "demo_api" }
                        });

                        // Register the Web providers integrations.
                        //
                        // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
                        // address per provider, unless all the registered providers support returning an "iss"
                        // parameter containing their URL as part of authorization responses. For more information,
                        // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
                        options.UseWebProviders()
                        
                               .AddGitHub(options =>
                               {
                                   options.SetClientId("8abc54b6d5f4e39d78aa")
                                          .SetClientSecret("f37ef38bdb18a0f5f2d430a8edbed4353c012dc3")
                                          // Note: GitHub doesn't support the recommended ":/" syntax and requires using "://".
                                          .SetRedirectUri("com.openiddict.sandbox.uno.client://callback/login/github");
                               });
                    });

                // Register the worker responsible for creating the database used to store tokens
                // and adding the registry entries required to register the custom URI scheme.
                //
                // Note: in a real world application, this step should be part of a setup script.
                services.AddHostedService<Worker>();
            },
            "OpenIddict.Sandbox.Uno.Client");
          // Add navigation support for toolkit controls such as TabBar and NavigationView

         builder.UseToolkitNavigation()
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
                            throw new ArgumentNullException(options?.ClientID,nameof(options.ClientID));
                        }
                        httpClient.DefaultRequestHeaders.Add("x-api-key", options.ClientID);
                    })
                )
                .AddRefitClient<IEtsyUserEndpoints>(context);
            })
            .UseAuthentication(authBuilder =>
            // reference used: https://github.com/unoplatform/uno.extensions/blob/main/testing/TestHarness/TestHarness/Ext/Authentication/Web/WebAuthenticationHostInit.cs
                authBuilder.AddWeb<IEtsyOAuthEndpoints>(configureWeb =>
                configureWeb
                    .LoginStartUri("https://openapi.etsy.com/v3/public/oauth/connect")
                    .AccessTokenKey(OAuthTokenRefreshDefaults.AccessTokenKey)
                    .RefreshTokenKey(OAuthTokenRefreshDefaults.RefreshToken)
                    .PrefersEphemeralWebBrowserSession(true)
                    .LoginCallbackUri("https://localhost:5001/etsy-auth-callback")
                    .PrepareLoginCallbackUri(
                        async(service,serviceProvider,tokencache,loginCallbackUri,ct)
                        => loginCallbackUri!)

                    .PrepareLoginStartUri(async (sp, tokens, credentials, loginStartUri, ct)
                        => await CreateLoginStartUri(sp, tokens, credentials, loginStartUri, ct))
                
                    .PostLogin(async(authService, serviceProvider,tokenCache, credentials, redirectUri, tokens,cancellationToken)
                        => await ProcessPostLoginAsync(authService, serviceProvider,tokenCache,credentials,redirectUri,tokens, cancellationToken))
                    
                    .Refresh(async (authService, serviceProvider, tokenCache, tokens, cancellationToken) =>
                        await RefreshTokensAsync(authService, serviceProvider, tokenCache, tokens, cancellationToken))

                    ,name: "EtsyOAuth"), // not sure if its possible to integrate OpenIddict with Uno.Extensions.Web.WebAuthenticationProvider
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
                services.AddSingleton<IHelpers, Helpers>();
                services.AddSingleton<ITasksManager, TasksManager>();
                services.AddSingleton<EtsyOAuthAuthenticationDelegate>();
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
                await navigator.NavigateViewModelAsync<AuthModel>(this, qualifier: Qualifiers.Nested);
            }
        });
    }
    #region Authentication Handlers
    private string? _stateBackingField;
    private string? _codeVerifierBackingField;
    internal async ValueTask<string> CreateLoginStartUri(
       IServiceProvider services,
       ITokenCache tokens,
       IDictionary<string, string>? credentials, // if this is null, can we use it for storing state and code verifier?
       string? loginStartUri,
       CancellationToken cancellationToken)
    {
        var options = services.GetRequiredService<IOptions<OAuthOptions>>().Value;
        credentials ??= new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(loginStartUri))
            loginStartUri = options.AuthorizationEndpoint!;
        
        var scope = string.Join(' ', options.Scopes);
        var state = OAuth2Utilitys.GenerateState();
        var codeVerifier = OAuth2Utilitys.GenerateCodeVerifier();
        var codeChallenge = OAuth2Utilitys.GenerateCodeChallenge(codeVerifier);

        var url = new UriBuilder(loginStartUri);
        var sb = new StringBuilder();
        void add(string k, string? v)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(k)).Append('=').Append(Uri.EscapeDataString(v ?? string.Empty));
        }
        add(OAuthAuthRequestDefaults.ResponseTypeKey, OAuthAuthRequestDefaults.CodeKey);
        add(OAuthAuthRequestDefaults.ClientIdKey, options.ClientID);
        add(OAuthAuthRequestDefaults.RedirectUriKey, options.RedirectUri);
        add(OAuthAuthRequestDefaults.ScopeKey, scope);
        add(OAuthAuthRequestDefaults.StateKey, state);
        add(OAuthPkceDefaults.CodeChallengeKey, codeChallenge);
        add(OAuthPkceDefaults.CodeChallengeMethodKey, OAuthPkceDefaults.CodeChallengeMethodS256);

        url.Query = sb.ToString();

        credentials.AddOrReplace(OAuthAuthRequestDefaults.StateKey, state);
        credentials.AddOrReplace(OAuthPkceDefaults.CodeVerifierKey, codeVerifier);

        // in case the credentials is null better use backing fields
        _stateBackingField = state;
        _codeVerifierBackingField = codeVerifier;

        return url.Uri.ToString();
    }

    // No idea how to instead integrate this to a seperate service but even then no idea how to open that damn browser
    private async ValueTask<IDictionary<string, string>?> ProcessPostLoginAsync(
        IEtsyOAuthEndpoints authEndpoints,
        IServiceProvider serviceProvider,
        ITokenCache tokenCache,
        IDictionary<string, string>? credentials,
        string redirectUri,
        IDictionary<string,string> tokens,
        CancellationToken ct = default)
    {
        var options = serviceProvider.GetRequiredService<IOptions<OAuthOptions>>().Value;
        if(credentials is null)
        {
            Log.Error("Credentials are null, cannot process post login. Will try to use backing fields.");
        }

        // Try to get state and codeVerifier from credentials, otherwise use backing fields
        string? state = null;
        string? codeVerifyer = null;
        if (credentials != null)
        {
            credentials.TryGetState(out state);
            credentials.TryGetCodeVerifier(out codeVerifyer);
        }
        if (string.IsNullOrEmpty(state))
            state = _stateBackingField;
        if (string.IsNullOrEmpty(codeVerifyer))
            codeVerifyer = _codeVerifierBackingField;
        // TODO: Write code to process credentials that are passed into the LoginAsync method
        if (!string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(codeVerifyer))
        {
            // Copyed from Uno.Extensions.Web.WebAuthenticationProvider
            var query = redirectUri.StartsWith(options.RedirectUri!)
                ? AuthHttpUtility.ExtractArguments(redirectUri)  // authData is a fully qualified url, so need to extract query or fragment
                : AuthHttpUtility.ParseQueryString(redirectUri.TrimStart('#').TrimStart('?')); // authData isn't full url, so just process as query or fragment

            // The redirectUri does hold the full response of the AuthorizationBrokerProvider
            // so its including all eventual query parameters that are not covered by the AccessToken or RefreshToken keys
            // if https://github.com/unoplatform/uno.extensions/pull/2893 gets merged, you could also provide additional query keys via 'OtherTokenKeys' in the appsettings section for WebAuthenticationProvider 'WebConfiguration' normally aliased with 'Web'
            var returnedState = query?.Get(OAuthAuthResponseDefaults.StateKey); // why is the Get not working while the uno extensions is using it the same?
            var authorizationCode = query?.Get(OAuthAuthResponseDefaults.CodeKey);
            var error = query?.Get(OAuthErrorResponseDefaults.ErrorKey);
            var errorDescription = query?.Get(OAuthErrorResponseDefaults.ErrorDescriptionKey);
            var errorUri = query?.Get(OAuthErrorResponseDefaults.ErrorUriKey);

            // Validate state and code
            if (string.IsNullOrWhiteSpace(state) || returnedState != state || string.IsNullOrWhiteSpace(authorizationCode))
            {
                Log.Error("Invalid state or code. State: '{state}', Old State: '{oldState}', Code: '{authCode}', Code Verifyer: {codeVerifier}",
                    returnedState, state, authorizationCode, codeVerifyer);
                return default;
            }

            TokenResponse tokenExchangeResult = await authEndpoints.ExchangeCodeAsync(new AccessTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.AuthorizationCode,
                ClientId = options.ClientID!,
                RedirectUri = options.RedirectUri!,
                Code = authorizationCode,
                CodeVerifier = codeVerifyer!
            });

            if(!(string.IsNullOrWhiteSpace(tokenExchangeResult.AccessToken) || string.IsNullOrWhiteSpace(tokenExchangeResult.RefreshToken)))
            {
                Log.Error("Failed to exchange code for access token!");
                return default;
            }
            // extract userId from refreshToken you may need it later
            var match = DoesContainUserId().Match(tokenExchangeResult.AccessToken!);
            if(match.Success)
            {
                string userId = match.Groups[1].Value;
            }

            var expirationTimeStamp = DateTime.Now.AddSeconds(tokenExchangeResult.ExpiresIn).ToString("g");
            // Save additional tokens if needed
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.AccessTokenKey, tokenExchangeResult.AccessToken!);
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.RefreshToken, tokenExchangeResult.RefreshToken!);
            tokens.AddOrReplace(OAuthTokenRefreshExtendedDefaults.ExpirationDateKey, expirationTimeStamp);

            // remove the state and code verifier from credentials as they are no longer needed
            if (!credentials.TryRemoveKeys([OAuthAuthRequestDefaults.StateKey, OAuthPkceDefaults.CodeVerifierKey]))
            { 
                Log.Warning("Failed to remove state and code verifier from credentials. Credentials was null? {Credentials}", credentials == null);
            }
            return tokens;
        }

        // Return null/default to fail the LoginAsync method
        return default;
    }
    
    private async ValueTask<IDictionary<string, string>?> RefreshTokensAsync(
        IEtsyOAuthEndpoints authEndpoints,
        IServiceProvider serviceProvider,
        ITokenCache tokenCache,
        IDictionary<string, string>? tokens,
        CancellationToken ct = default)
    {
        var options = serviceProvider.GetRequiredService<IOptions<OAuthOptions>>().Value;
        if(tokens is null)
        {
            Log.Error("Tokens are null, cannot refresh tokens.");
            return default;
        }

        // TODO: Write code to refresh tokens using the currently stored tokens
        if ((tokens?.TryGetRefreshToken(out var refreshToken) ?? false) && !refreshToken.IsNullOrWhiteSpace()
         && (tokens?.TryGetExpirationDate(out var tokenExpiry) ?? false) && tokenExpiry > DateTime.Now)
        {

            var tokenResponse = await authEndpoints.RefreshTokenAsync(new RefreshTokenRequest
            {
                GrantType = OAuthTokenRefreshDefaults.RefreshToken,
                ClientId = options.ClientID!,
                RefreshToken = refreshToken!
            });

            if (string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                Log.Error("Refresh response missing access_token or refresh_token.");
                return default;
            }

            // Return IDictionary containing any tokens used by service calls or in the app
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.AccessTokenKey, tokenResponse.AccessToken);
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.RefreshToken, tokenResponse.RefreshToken);
            tokens.AddOrReplace(OAuthTokenRefreshDefaults.ExpiresInKey, DateTime.Now.AddMinutes(5).ToString("g"));
            return tokens;
        }

        // Return null/default to fail the Refresh method
        return default;
    }

    #endregion
    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<AuthPage, AuthModel>(),
            new ViewMap<MainPage, MainModel>(),
            new DataViewMap<SecondPage, SecondModel, Entity>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    
                    new ("Main", View: views.FindByViewModel<MainModel>(), IsDefault:true),
                    new ("Second", View: views.FindByViewModel<SecondModel>()),
                    new ("Auth", View: views.FindByViewModel<AuthModel>()),
                ]
            )
        );
    }

    [GeneratedRegex(@"^(\d+)\.")]
    private static partial Regex DoesContainUserId();
}
