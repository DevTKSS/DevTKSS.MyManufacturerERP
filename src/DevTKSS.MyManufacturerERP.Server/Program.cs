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

    // Add services to the container.
    //builder.Services.Configure<JsonOptions>(options =>
    //            // Configure the JsonSerializerOptions to use the generated WeatherForecastContext
    //            options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
    //            TodoItemContext.Default,
    //            WeatherForecastContext.Default));
    
    builder.Services.Configure<RouteOptions>(options =>
                // Configure the RouteOptions to use lowercase URLs
        options.LowercaseUrls = true);

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document,_,_) =>
        {

            document.Info = new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "MyManufacturerERP API",
                Version = "v1",
                Description = "This is the API for MyManufacturerERP, a sample application demonstrating ASP.NET Core Minimal APIs with Uno Platform."
                //Contact = new Microsoft.OpenApi.Models.OpenApiContact
                //{
                //    Name = "DevTKSS",
                //    Email = "info@technische-konstruktion.com",
                //    Url = new Uri("https://www.technische-konstruktion.com")
                //}
            };
            return Task.CompletedTask;
        });
        options.AddScalarTransformers();
    });

    // DbContexts
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("AuthDb"));
    builder.Services.AddDbContext<TodoDb>(options =>
        options.UseInMemoryDatabase("TodoList"));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Identity
    builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddSignInManager()
        .AddDefaultTokenProviders()
        .AddRoles<IdentityRole>()
        .AddUserConfirmation<DefaultUserConfirmation<ApplicationUser>>()
        .AddUserManager<UserManager<ApplicationUser>>()
        .AddErrorDescriber<IdentityErrorDescriber>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    // Auth/CORS/Antiforgery
    builder.Services.AddSecurityServices(builder.Configuration);
    builder.Services.AddAuthServices(builder.Configuration);
    builder.Services.AddAuthorization();

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
    app.UseCors();
    app.UseCookiePolicy();
    app.UseAntiforgery();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseUnoFrameworkFiles(); // Mysterious Uno function, no one tells you what it does,
                                // but it is required to maybe serve/host the WebAssembly Target in Uno Platform applications.
    app.UseStaticFiles();
    app.UseSerilogRequestLogging(); // Recommendation from Serilog.AspNetCore to log HTTP requests
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

    // This **Should** map the Identity API endpoints for IdentityUser,
    // but does **NOT** publish any endpoints looking at the Endoint Explorer
    // I think this is the reason why the login of the client application does not work.
    // Map Identity minimal API endpoints (cookie auth by default)
    app.MapIdentityApi<ApplicationUser>()
        .WithTags("Identity")
        .WithOpenApi();

    app.MapGet("/error", () => "An unexpected Error occured!")
        .AllowAnonymous()
        .WithName("Error")
        .WithOpenApi();
    app.MapGet("/forbidden", () => "You do not have permission to access this resource.")
        .AllowAnonymous()
        .WithName("Forbidden")
        .WithOpenApi();

    app.MapGet("/", () => "Welcome to MyManufacturerERP API!")
        .WithName("Welcome")
        .AllowAnonymous()
        .WithOpenApi();

    app.MapFallbackToFile("index.html");
    app.MapFallback(() => "/scalar/v1"); // Fallback to the Scalar API Reference
   
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

