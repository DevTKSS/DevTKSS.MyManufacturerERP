// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
// logger configured in `AddSerilog()` below, once configuration and dependency-injection have both been
// set up successfully.

using System.Net;

using Microsoft.AspNetCore.DataProtection;

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
    // Add services to the container.
    //builder.Services.Configure<JsonOptions>(options =>
    //    // Configure the SerializerOptions to use the generated TodoItemContext
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
                    .AddEntityFrameworkStores<AuthDbContext>();
    #endregion

    #region Add Authentication and Authorization Services
    builder.Services.AddAntiforgery();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddOAuth("Etsy", options =>
        {
            var etsyConfig = builder.Configuration
                                    .GetSection("Authentication")
                                    .GetSection("Etsy");
            options.ClientId = etsyConfig["ClientId"] ?? throw new ArgumentNullException(nameof(options.ClientId));
            options.ClientSecret = etsyConfig["ClientSecret"] ?? throw new ArgumentNullException(nameof(options.ClientSecret));
            options.CallbackPath = new PathString(etsyConfig.GetValue<PathString>("CallbackPath","/etsy-auth-callback"));
            options.AuthorizationEndpoint = etsyConfig.GetValue("AuthorizationEndpoint", "https://www.etsy.com/oauth/connect");
            options.TokenEndpoint = etsyConfig.GetValue("TokenEndpoint", "https://api.etsy.com/v3/public/oauth/token");
            options.UserInformationEndpoint = etsyConfig.GetValue("UserInformationEndpoint", "https://api.etsy.com/v3/application/users/me");
            options.UsePkce = etsyConfig.GetValue("UsePkce", true);

            options.Events = new OAuthEvents
            {
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/Home/Error?message=" + context.Failure?.Message);
                    context.HandleResponse();
                    return Task.CompletedTask;
                }

            };

            var scopes = etsyConfig.GetValue("Scope", string.Empty)
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
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapFallback(() => "/scalar/v1");
    app.MapTodoItemApi();


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
                .WithDocumentDownloadType(DocumentDownloadType.Json)
                .WithFavicon("/Assets/styled-logo.ico")
                .AddMetadata("title","MyManufacturerERP API Reference Title")
                .AddMetadata("ogImage", "/Assets/styled-logo-small.png");
        }); 
    }

    // Identity endpoints with OpenAPI and tags
    app.MapIdentityApi<User>()
        .WithTags("Identity")
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
