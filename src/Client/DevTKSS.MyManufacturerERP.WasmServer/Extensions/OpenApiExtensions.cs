// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
// logger configured in `AddSerilog()` below, once configuration and dependency-injection have both been
// set up successfully.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

internal static class OpenApiExtensions
{ 
    public static IServiceCollection AddOpenApiProvider(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((OpenApiDocument document, OpenApiDocumentTransformerContext _, CancellationToken _) =>
            {

                document.Info = new Microsoft.OpenApi.OpenApiInfo
                {
                    Title = "MyManufacturerERP API",
                    Version = "v1",
                    Description = "This is the API for MyManufacturerERP, a sample application demonstrating ASP.NET Core Minimal APIs with Uno Platform.",
                    Contact = new Microsoft.OpenApi.OpenApiContact
                    {
                        Name = "DevTKSS",
                        Email = "info@technische-konstruktion.com",
                        Url = new Uri("https://www.technische-konstruktion.com")
                    }
                };
                return Task.CompletedTask;
            });
            options.AddScalarTransformers();
        });
        return services;
    }
    public static WebApplication MapOpenApiProvider(this WebApplication app)
    {
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
                    .WithClassicLayout()
                    .AddPreferredSecuritySchemes("Cookie")
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .WithDocumentDownloadType(DocumentDownloadType.Json);
            });
        }
        return app;
    }
   
}