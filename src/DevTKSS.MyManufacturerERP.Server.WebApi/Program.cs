// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
// logger configured in `AddSerilog()` below, once configuration and dependency-injection have both been
// set up successfully.

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

    builder.Services.Configure<RouteOptions>(options =>
            // Configure the RouteOptions to use lowercase URLs
            options.LowercaseUrls = true);

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi(options =>
    {
        options.AddScalarTransformers();
    });

    #region Add DbContext Services
    builder.Services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseInMemoryDatabase("AuthDb");
        });
    builder.Services.AddDbContext<TodoDb>(options =>
        {
            options.UseInMemoryDatabase("TodoList");
        }); 

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddIdentityApiEndpoints<User>()
        .AddSignInManager()
        .AddDefaultTokenProviders()
        .AddRoles<IdentityRole>()
        .AddUserConfirmation<DefaultUserConfirmation<User>>()
        .AddUserManager<UserManager<User>>()
        .AddErrorDescriber<IdentityErrorDescriber>()
        .AddEntityFrameworkStores<AuthDbContext>();
    #endregion

    #region Add Authentication and Authorization Services
    builder.Services.AddAntiforgery();
    builder.Services.AddCors();
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie()
        .AddOAuth("Etsy", options =>
        {
            var etsyConfig = builder.Configuration
                                  .GetSection("Authentication")
                                  .GetSection("Etsy");

            options.ClientId = etsyConfig["ClientId"] ?? throw new ArgumentNullException(options.ClientId);
            options.ClientSecret = etsyConfig["ClientSecret"] ?? throw new ArgumentNullException(options.ClientSecret);
            options.CallbackPath = new PathString(etsyConfig.GetValue<string>("CallbackPath", "/etsy-auth-callback"));
            options.AuthorizationEndpoint = etsyConfig.GetValue<string>("AuthorizationEndpoint", "https://www.etsy.com/oauth/connect");
            options.TokenEndpoint = etsyConfig.GetValue<string>("TokenEndpoint", "https://api.etsy.com/v3/public/oauth/token");
            options.UserInformationEndpoint = etsyConfig.GetValue<string>("UserInformationEndpoint", "https://api.etsy.com/v3/application/users/me");
            options.UsePkce = etsyConfig.GetValue<bool>("usePkce", true);

            options.Events = new OAuthEvents
            {
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/error?message=" + context.Failure?.Message);
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };

            var scopes = (etsyConfig.GetValue("Scope", string.Empty) ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var scope in scopes)
            {
                options.Scope.Add(scope);
            }

            options.SaveTokens = true;
            options.Validate();
        });
    builder.Services.AddAuthorization();

    #endregion


    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseCookiePolicy();
    app.UseAntiforgery();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Map("/api", (HttpContext context) => context.Response.Redirect("/scalar/v1"))
        .WithName("ApiReference")
        .WithDisplayName("Api Reference")
        .WithDescription("Redirects to the Scalar API Reference documentation.");

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
    app.MapTodoItemApi();
    // Identity endpoints with OpenAPI and tags
    app.MapIdentityApi<User>()
        .WithTags("Identity")
        .WithOpenApi();
    app.MapGet("/error", () => "An unexpected Error occured!")
        .AllowAnonymous()
        .WithName("Error")
        .WithOpenApi();
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
