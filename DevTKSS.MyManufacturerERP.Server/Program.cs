using System.Text.Json.Serialization.Metadata;
using DevTKSS.MyManufacturerERP.DataContracts.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
// using Uno.Wasm.Bootstrap.Server;

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
            formatter:new ExpressionTemplate(
                // Include trace and span IDs when present.
                template: "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
                theme: TemplateTheme.Code
            ))
         );

    // Add services to the container.
    builder.Services.Configure<JsonOptions>(options =>
                // Configure the JsonSerializerOptions to use the generated WeatherForecastContext
                options.JsonSerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
                WeatherForecastContext.Default));

    builder.Services.Configure<RouteOptions>(options =>
                // Configure the RouteOptions to use lowercase URLs
                options.LowercaseUrls = true);

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

#if DEBUG // this current approach is build along Youtube video: https://www.youtube.com/watch?v=V-S5JZJUvvU
    builder.Services.AddDbContext<AuthDbContext>(options =>
    {
        options.UseInMemoryDatabase("AuthDb");
    });

    builder.Services.AddAuthorization();
    builder.Services.AddIdentityApiEndpoints<IdentityUser>()
        .AddEntityFrameworkStores<AuthDbContext>();

    builder.Services.AddAuthentication(configureOptions =>
    {
        configureOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        configureOptions.DefaultChallengeScheme = OAuthDefaults.DisplayName;
    })
    .AddCookie()
    .AddOAuth("Etsy", options =>
    {
        var EtsyConfig = builder.Configuration.GetSection("Authentication:Etsy");
        options.UsePkce = true;
        options.ClientId = EtsyConfig["ClientId"];
        options.ClientSecret = EtsyConfig["ClientSecret"];
        options.CallbackPath = EtsyConfig["CallbackPath"];
        options.AuthorizationEndpoint = EtsyConfig["Authorization"];
        options.TokenEndpoint = EtsyConfig["Token"];
        options.UserInformationEndpoint = EtsyConfig["UserInformation"];
        options.Events = new OAuthEvents
        {
            OnRemoteFailure = context =>
            {
                context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
                context.HandleResponse();
                return Task.CompletedTask;
            }

        };
        var scopeString = EtsyConfig["Scope"];
        if (!string.IsNullOrWhiteSpace(scopeString))
        {
            foreach (var scope in scopeString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                options.Scope.Add(scope);
            }
        }
        options.SaveTokens = true;
        options.Validate();
        // Optional: Map user claims etc.
    });
    //builder.Services.AddIdentityCore<User>()
    //    .AddEntityFrameworkStores<AuthDbContext>()
    //    .AddApiEndpoints();
#endif

    var app = builder.Build();
    
    app.MapIdentityApi<IdentityUser>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("MyManufacturerERP API Reference")
                    .WithTheme(ScalarTheme.Mars)
                    .WithDarkModeToggle(true)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .WithLayout(ScalarLayout.Modern)
                    .WithDocumentDownloadType(DocumentDownloadType.Both);
            
        });
    }

    app.UseHttpsRedirection();
    // app.UseUnoFrameworkFiles();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging(); // Recommendation from Serilog.AspNetCore to log HTTP requests

    app.MapFallbackToFile("index.html");

    //app.MapWeatherApi();

    await app.RunAsync();
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
