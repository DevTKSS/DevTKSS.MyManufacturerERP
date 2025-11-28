using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Options;

namespace DevTKSS.MyManufacturerERP.Server.Api.Identity;

public static class AuthenticationFeature
{
    public static WebApplication MapAuthenticationFeatures(this WebApplication app)
    {
        // Apply CORS policy at the group level and our allowed-origin endpoint filter
        var authGroup = app.MapGroup("/auth")
                           .RequireHost([.. app.Services.GetRequiredService<IOptions<HostFilteringOptions>>().Value.AllowedHosts])
                           .WithTags("Authentication")
                           .RequireCors("MyCorsPolicy");

        var signin = app.MapGroup("/signin").RequireCors("MyCorsPolicy");

        // Connectors endpoint returns available external providers
        authGroup.MapGet("/connectors", (Delegate)GetConnectors)
            .AllowAnonymous();

        // Endpoint to get antiforgery token for Minimal API clients (POST only)
        authGroup.MapPost("/token", Token)
                .WithDescription("Get an anti-forgery token to use in subsequent requests.")
                .WithSummary("Get Anti-Forgery Token")
                .WithTags("Token")
                .AllowAnonymous();

        // Signin endpoint: initiate external auth challenge
        signin.MapGet("/{provider}", Signin)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetConnectors(HttpContext context)
        => TypedResults.Ok(await context.GetExternalProvidersAsync());

    private static IResult Token(IAntiforgery antiforgery, HttpContext context)
    {
        // return anti-forgery token to the caller
        var tokens = antiforgery.GetAndStoreTokens(context);
        return TypedResults.Ok(new { token = tokens.RequestToken });
    }

    private static async Task<IResult> Signin(string provider, HttpContext http)
    {
        // Map provider id (lowercase) to authentication scheme DisplayName
        var providerDisplay = EtsyAuthenticationDefaults.DisplayName;
        var supported = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { providerDisplay.ToLowerInvariant(), providerDisplay }
        };

        if (!supported.TryGetValue(provider, out var scheme))
        {
            return TypedResults.BadRequest(new ErrorResponse("unsupported_provider", "The requested provider is not supported."));
        }

        await http.ChallengeAsync(scheme, new AuthenticationProperties
        {
            RedirectUri = "/"
        });

        return TypedResults.StatusCode(StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> RefreshToken(HttpContext http)
    {
        var authResult = await http.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
        {
            return TypedResults.Unauthorized();
        }

        var refreshToken = authResult.Properties.GetTokenValue("refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
        {
            return TypedResults.BadRequest(new ErrorResponse("no_refresh_token", "No refresh token available."));
        }

        // Attempt to use authentication handler's saved tokens - for cookie-based external providers
        // Some handlers automatically refresh tokens when configured; here we simply return the stored
        // tokens back to the client as a convenience.
        var access = authResult.Properties.GetTokenValue("access_token");

        return TypedResults.Ok(new { access_token = access, refresh_token = refreshToken });
    }
}
