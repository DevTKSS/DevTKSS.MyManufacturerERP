namespace DevTKSS.MyManufacturerERP.Server.Extensions;

public static class SecurityServiceExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {

        // Etsy Authentication requires Cross-site request forgery (CSRF) protection,
        // <see href="https://developers.etsy.com/documentation/essentials/authentication#requesting-an-oauth-token"/>
        // so we need to add the Antiforgery service.
        services.AddAntiforgery(setupAction =>
        {
            setupAction.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Always use HTTPS
        });

        // Add CORS service policy as needed for wasm Multi-threaded applications
        // <see href="https://github.com/dotnet/runtime/blob/main/src/mono/wasm/features.md#multi-threading"/>
        // Configuration reference:
        // <see href="https://substack.com/home/post/p-164745710"/>
        services.AddCors(options =>
        {
            var AllowedHosts = configuration["AllowedHosts"]?
                                              .ToLowerInvariant().Split(';', StringSplitOptions.RemoveEmptyEntries)
                                              ?? throw new ArgumentNullException("AllowedHosts"); ;

            string[] Methods = [
                HttpMethods.Get,
                HttpMethods.Post,
                HttpMethods.Put,
                HttpMethods.Delete,
                HttpMethods.Patch
            ];
            string[] Headers = [
                HeaderNames.ContentType,
                HeaderNames.Cookie, // Adding Cookie header to allow cookies in CORS requests, as I want to use Cookie Authentication
                HeaderNames.Authorization,
                // Adding headers referring to MDN Docs
                // <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CORS" />
                // to avoid problems with browsers like Safari or Nightly
                HeaderNames.ContentLanguage,
                HeaderNames.AcceptLanguage,
                HeaderNames.AcceptEncoding,
                // Adding Preflight request headers from samples
                // <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS#preflighted_requests"/>
                HeaderNames.Origin,
                HeaderNames.UserAgent,
                HeaderNames.AccessControlRequestHeaders,
                HeaderNames.AccessControlRequestMethod,
                HeaderNames.Host
            ];


            options.AddPolicy("MyCorsPolicy", policy =>
            {
                policy.WithOrigins()
                .WithMethods(Methods)
                .WithHeaders(Headers)
                .DisallowCredentials() // Credentials are not required for cors requests as we use cookies
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });

        });
        return services;
    }
}
