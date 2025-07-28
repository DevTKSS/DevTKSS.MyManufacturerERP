// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
// logger configured in `AddSerilog()` below, once configuration and dependency-injection have both been
// set up successfully.

using DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;
using DevTKSS.MyManufacturerERP.WebApi.Endpoints.Weather;

Log.Logger = new LoggerConfiguration()
      .WriteTo.Console()
      .CreateBootstrapLogger();

Log.Information("Starting up!");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure the application to use Serilog for logging
    builder.Host.UseSerilog((context, services, lc) => lc
         .ReadFrom.Configuration(context.Configuration)
         .ReadFrom.Services(services)
         .Enrich.FromLogContext()
         .WriteTo.Console(
            formatter: new ExpressionTemplate(
                // Include trace and span IDs when present.
                template: "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
                theme: TemplateTheme.Code
            ))
         );
    //  Add services to the container.
    //builder.Services.Configure<JsonOptions>(options =>
    //     Configure the SerializerOptions to use the generated TodoItemContext

    //    options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
    //        TodoItemContext.Default));

    builder.Services.AddHttpsRedirection(x =>
    {
        x.HttpsPort = 5000;
    });

    builder.Services.Configure<RouteOptions>(options =>
            // Configure the RouteOptions to use lowercase URLs
            options.LowercaseUrls = true);

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi(options =>
    {
        options.AddScalarTransformers();
    });

    #region Add DbContext Services
    builder.Services.AddDbContext<ApplicationDbContext>(x =>
    {
        x.UseInMemoryDatabase("AuthDb");
        x.UseOpenIddict();
    });

    //builder.Services.AddDbContext<AuthDbContext>(options =>
    //    {
    //        options.UseInMemoryDatabase("AuthDb");
    //    });
    builder.Services.AddDbContext<TodoDb>(options =>
        {
            options.UseInMemoryDatabase("TodoList");
        }); 

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    //builder.Services.AddIdentityApiEndpoints<User>()
    //    .AddSignInManager()
    //    .AddEntityFrameworkStores<AuthDbContext>();
    #endregion

    #region Add Authentication and Authorization Services old
    //builder.Services.AddAntiforgery();
    //builder.Services.AddCors();
    //builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    //    .AddCookie()
    //    .AddOAuth("Etsy", options =>
    //    {
    //        var etsyConfig = builder.Configuration
    //                              .GetSection("Authentication")
    //                              .GetSection("Etsy");

    //        options.ClientId = etsyConfig["ClientId"] ?? throw new ArgumentNullException(options.ClientId);
    //        options.ClientSecret = etsyConfig["ClientSecret"] ?? throw new ArgumentNullException(options.ClientSecret);
    //        options.CallbackPath = new PathString(etsyConfig.GetValue<string>("CallbackPath", "/etsy-auth-callback"));
    //        options.AuthorizationEndpoint = etsyConfig.GetValue<string>("AuthorizationEndpoint", "https://www.etsy.com/oauth/connect");
    //        options.TokenEndpoint = etsyConfig.GetValue<string>("TokenEndpoint", "https://api.etsy.com/v3/public/oauth/token");
    //        options.UserInformationEndpoint = etsyConfig.GetValue<string>("UserInformationEndpoint", "https://api.etsy.com/v3/application/users/me");
    //        options.UsePkce = etsyConfig.GetValue<bool>("usePkce", true);

    //        options.Events = new OAuthEvents
    //        {
    //            OnRemoteFailure = context =>
    //            {
    //                context.Response.Redirect("/error?message=" + context.Failure?.Message);
    //                context.HandleResponse();
    //                return Task.CompletedTask;
    //            }
    //        };

    //        var scopes = (etsyConfig.GetValue("Scope", string.Empty) ?? string.Empty)
    //            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

    //        foreach (var scope in scopes)
    //        {
    //            options.Scope.Add(scope);
    //        }

    //        options.SaveTokens = true;
    //        options.Validate();
    //    });
    //builder.Services.AddAuthorization();

    #endregion
    builder.Services
        .AddOpenIddict()
        .AddCore(x =>
        {
            // Register the Entity Framework Core stores and models.
            x.UseEntityFrameworkCore()
                .UseDbContext<ApplicationDbContext>();
        })
        .AddServer(x =>
        {
            x.SetAuthorizationEndpointUris("/connect/authorize")
             .SetTokenEndpointUris("/connect/token");

            x.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email);

            x.AllowAuthorizationCodeFlow()
             .RequireProofKeyForCodeExchange();

            x.AddEphemeralEncryptionKey()
             .AddEphemeralSigningKey()
             .DisableAccessTokenEncryption();

            x.UseAspNetCore()
             .EnableAuthorizationEndpointPassthrough()
             .EnableTokenEndpointPassthrough();
        })
        .AddValidation(options =>
        {
            // If the server and the APIs are in the same project, you can use UseLocalServer
            options.UseLocalServer();
            options.UseAspNetCore();
        });

    builder.Services.AddHostedService<ClientSeeder>();

    builder.Services.AddAuthentication(options =>
    {
        // set the default scheme for token validation
        options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    }).AddCookie(); // if you also want to keep cookie support
    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    //app.UseCors();
    //app.UseCookiePolicy();
    //app.UseAntiforgery();
    app.UseAuthentication();
    app.UseAuthorization();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("MyManufacturerERP API Reference")
                .WithTheme(ScalarTheme.Saturn)
                .WithLayout(ScalarLayout.Modern)
                .WithDarkModeToggle(true)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithDocumentDownloadType(DocumentDownloadType.Json);
        }); 
    }
    app.Map("/api", (HttpContext context) => context.Response.Redirect("/scalar/v1"))
        .WithName("ApiReference")
        .WithDisplayName("Api Reference")
        .WithDescription("Redirects to the Scalar API Reference documentation.");

    //// Identity endpoints with OpenAPI and tags
    //app.MapIdentityApi<User>()
    //    .WithTags("Identity")
    //    .WithOpenApi();
    app.MapGet("/error", () => "An unexpected Error occured!")
        .AllowAnonymous()
        .WithName("Error")
        .WithOpenApi();

    app.MapTodoEnpoints();
    app.MapWeatherEndpoints();
    // app.MapAuthenticationEndpoints(); // TODO: Implement this endpoint by fixing the lintings in /Endpoints/Authentication
    // <see href="https://github.com/dotnet/AspNetCore.Docs/issues/35835#issuecomment-3128169445" />
    // If done, remove the following mappings and use the refactored method here instead.
    app.MapGet("/connect/authorize", async context =>
    {
        var request = context.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("Invalid request");

        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name, Claims.Role);
        identity.AddClaim(Claims.Subject, "dummy_user_id");
        identity.AddClaim(Claims.Name, "Test User");

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email);

        await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);

        context.Response.Clear();

        var template = await File.ReadAllTextAsync("page.html");
        var code = Uri.EscapeDataString(context.GetOpenIddictServerResponse()!.Code!);
        var state = Uri.EscapeDataString(context.GetOpenIddictServerResponse()!.State!);
        var redirect = new UriBuilder(request.RedirectUri!)
        {
            Query = $"code={code}&state={state}"
        }.Uri.ToString();

        var html = string.Format(template, redirect);

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html);
    });

    app.MapPost("/connect/token", async context =>
    {
        var request = context.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("Invalid request.");

        if (request.IsAuthorizationCodeGrantType())
        {
            // Normally you retrieve the principal associated with the code.
            // For simplicity, here you recreate it – in production, check that the code has not
            // already been consumed and perform all necessary validations.
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(Claims.Subject, "dummy_user_id");
            identity.AddClaim(Claims.Name, "Test User");

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
        }
        else
        {
            // If the grant type is not recognized, trigger a Challenge or
            // return an error.
            await context.ChallengeAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    });

    app.MapGet("/connect/userinfo", async context =>
    {
        // Check that the request is authenticated
        var user = context.User;

        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized.");
            return;
        }

        // Create the object to return. You can include more claims if necessary.
        var userInfo = new
        {
            sub = user.FindFirst(Claims.Subject)?.Value,
            name = user.FindFirst(Claims.Name)?.Value,
            email = user.FindFirst(Claims.Email)?.Value
        };

        // Return the JSON with the user's information
        await context.Response.WriteAsJsonAsync(userInfo);
    });

    #region Endpoint reference
    //var summaries = new[]
    //{
    //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    //};

    //app.MapGet("/weatherforecast", () =>
    //{
    //    var forecast = Enumerable.Range(1, 5).Select(index =>
    //        new WeatherForecast
    //        (
    //            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    //            Random.Shared.Next(-20, 55),
    //            summaries[Random.Shared.Next(summaries.Length)]
    //        ))
    //        .ToArray();
    //    return forecast;
    //})
    //.WithName("GetWeatherForecast");
    #endregion
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
#if DEBUG
    if (System.Diagnostics.Debugger.IsAttached)
    {
        System.Diagnostics.Debugger.Break();
    }
#endif
}
finally
{
    Log.CloseAndFlush();
}

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
