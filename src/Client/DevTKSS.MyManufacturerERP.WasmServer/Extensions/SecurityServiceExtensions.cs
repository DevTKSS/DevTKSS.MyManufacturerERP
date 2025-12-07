using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Options;

namespace DevTKSS.MyManufacturerERP.Server.Extensions;

public static class SecurityServiceExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        string[] allowedHosts = [];
        // Configure AllowedHosts for HostFiltering from configuration (appsettings.json -> AllowedHosts)
        services.Configure<HostFilteringOptions>(options =>
        {
            var allowedHostConfig = configuration["AllowedHosts"];
            if (!string.IsNullOrEmpty(allowedHostConfig))
            {
                allowedHosts = allowedHostConfig.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? throw new ArgumentNullException(nameof(allowedHosts));
                options.AllowedHosts = allowedHosts;
            }
        });
        // Antiforgery: required when using cookie-based flows or server-side form posts
        services.AddAntiforgery(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            options.HeaderName = "X-CSRF-TOKEN";
        });

        // Add CORS service policy as needed for wasm Multi-threaded applications
        // <see href="https://github.com/dotnet/runtime/blob/main/src/mono/wasm/features.md#multi-threading"/>
        // Configuration reference:
        // <see href="https://substack.com/home/post/p-164745710"/>
        services.AddCors(options =>
        {

            options.AddPolicy("MyCorsPolicy", policy =>
            {
                policy
                    .WithOrigins(allowedHosts)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                    .WithHeaders(
                        HeaderNames.ContentType,
                        HeaderNames.Authorization,
                        HeaderNames.AcceptLanguage,
                        HeaderNames.AcceptEncoding,
                        "X-CSRF-TOKEN")
                    // If using cookie authentication from browser, enable credentials
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });

        });
        return services;
    }
}
