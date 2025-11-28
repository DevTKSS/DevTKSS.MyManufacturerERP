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

    builder.Services.Configure<RouteOptions>(options =>
        // Configure the RouteOptions to use lowercase URLs
        options.LowercaseUrls = true);

    builder.Services.AddOpenApiProvider();

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Auth/CORS/Antiforgery
    builder.Services.AddSecurityServices(builder.Configuration);
    builder.Services.AddAuthServices(builder.Configuration);
    builder.Services.AddAuthorization();

    // Register AllowedOriginsProvider for auth minimal API group origin checks
    builder.Services.AddSingleton<IAllowedOriginsProvider, AllowedOriginsProvider>();
    builder.Services.AddHttpClient();

    var app = builder.Build();

    // app.Urls.Add("https://localhost:5001"); // Add a URL for HTTPS redirection

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint(); // Show migrations endpoint in development
    }
    else
    {
        app.UseExceptionHandler("/error"); // Handle exceptions in production
        app.UseHsts(); // Use HTTP Strict Transport Security
    }
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    // Use named CORS policy
    app.UseCors("MyCorsPolicy");

    app.UseCookiePolicy();

    // Note: Antiforgery automatically validates tokens when you use the IAntiforgery.ValidateRequestAsync
    // call in endpoints. For minimal APIs we expose a token endpoint below the client can call to get the token.

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseUnoFrameworkFiles(); // Mysterious Uno function, no one tells you what it does,
                                // but it is required to maybe serve/host the WebAssembly Target in Uno Platform applications.
    app.UseStaticFiles();
    app.UseSerilogRequestLogging(); // Recommendation from Serilog.AspNetCore to log HTTP requests
    app.MapOpenApiProvider();

    app.MapGet("/error", () => "An unexpected Error occured!")
        .AllowAnonymous()
        .WithName("Error");
    app.MapGet("/forbidden", () => "You do not have permission to access this resource.")
        .AllowAnonymous()
        .WithName("Forbidden");

    // Connectors + auth endpoints moved to feature extension
    app.MapAuthenticationFeatures();

    app.MapFallbackToFile("index.html");
    // app.MapFallback(() => "/scalar/v1"); // Fallback to the Scalar API Reference   
    app.MapTodoItemApi(); // Map the TodoItem API endpoints generated via the Minimal-API Tutorial

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

