// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
// logger configured in `AddSerilog()` below, once configuration and dependency-injection have both been
// set up successfully.

using DevTKSS.MyManufacturerERP.Server.Extensions;

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

    //// Add services to the container.
    builder.Services.Configure<JsonOptions>(options =>
                // Configure the JsonSerializerOptions to use the generated WeatherForecastContext
                options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
                WeatherForecastContext.Default,
                TodoItemContext.Default));

    builder.Services.Configure<RouteOptions>(options =>
                // Configure the RouteOptions to use lowercase URLs
                options.LowercaseUrls = true);

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi(options =>
        options.AddScalarTransformers()
    );


    
    // Definition from the TodoList example of the Minimal APIs Tutorial
    // <see href="https://learn.microsoft.com/de-de/aspnet/core/tutorials/min-web-api?view=aspnetcore-9.0&tabs=visual-studio" />
    builder.Services.AddDbContext<TodoDb>(opt =>
        {
            opt.UseInMemoryDatabase("TodoList");
        });

    // This AuthDb approach is build along Youtube video: https://www.youtube.com/watch?v=V-S5JZJUvvU
    builder.Services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseInMemoryDatabase("AuthDb");
        });

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    #region Refactored to /Extensions Directory for brevity
    builder.Services.AddSecurityServices(builder.Configuration);
    builder.Services.AddAuthServices(builder.Configuration);
    #endregion

    builder.Services.AddIdentityApiEndpoints<User>()
        .AddEntityFrameworkStores<AuthDbContext>();

    var app = builder.Build();

    // Would be genius to be able to use a error page without the need of Razor pages.
    // But I don't know how to do this with Uno Platform as there is no Server Project Documentation available
    // that could be used as Reference, only the ASP.NET Core documentation, which tells you to use Razor pages or MVC Controllers.
    //if(app.Environment.IsDevelopment())
    //{
    //    app.UseExceptionHandler("/Error");
    //}

    app.UseHttpsRedirection();

    app.UseSecurityServices(); // Register the security services, like CORS, Antiforgery, etc.
    app.UseAuthServices(); // Register the authentication services, like Cookie Authentication, OAuth, etc.

    app.UseUnoFrameworkFiles(); // Mysterious Uno function, no one tells you what it does,
                                // but it is required to maybe serve/host the WebAssembly Target in Uno Platform applications.
    app.UseStaticFiles();
    app.UseSerilogRequestLogging(); // Recommendation from Serilog.AspNetCore to log HTTP requests
    app.UseWelcomePage("/Home"); // This is a simple welcome page that shows the application is running

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

    // This **Should** map the Identity API endpoints for IdentityUser,
    // but does **NOT** publish any endpoints looking at the Endoint Explorer
    // I think this is the reason why the login of the client application does not work.
    app.MapIdentityApi<User>()
        .WithGroupName("Identity")
        .WithOpenApi(); 

    app.MapFallbackToFile("index.html");
    app.MapFallback(() => "/scalar/v1"); // Fallback to the Scalar API Reference
    app.MapWeatherApi();
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
