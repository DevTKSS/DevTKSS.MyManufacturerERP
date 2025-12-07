using System.Security.Claims;

namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;

/// <summary>
/// OAuth2 authentication endpoints for Etsy integration.
/// Handles login, logout, and user profile information.
/// </summary>
public static class OAuthEndpoints
{
    public static void MapOAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("OAuth");

        group.MapGet("/login", LoginAsync)
            .WithName("GetOAuthLogin")
            .WithSummary("Start OAuth Login")
            .WithDescription("Initiates OAuth2 flow by redirecting to Etsy login")
            .AllowAnonymous();

        group.MapGet("/logout", (Delegate)LogoutAsync)
            .WithName("GetOAuthLogout")
            .WithSummary("Logout")
            .WithDescription("Signs out the user and clears authentication cookies")
            .RequireAuthorization();

        group.MapGet("/profile", GetProfileAsync)
            .WithName("GetOAuthProfile")
            .WithSummary("Get Current User Profile")
            .WithDescription("Returns the current authenticated user's information")
            .RequireAuthorization();

        group.MapGet("/callback/etsy", (Delegate)HandleCallbackAsync)
            .WithName("GetOAuthCallback")
            .WithSummary("OAuth Callback Handler")
            .WithDescription("Handles the redirect from Etsy after user authentication")
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    /// <summary>
    /// GET /auth/login
    /// Initiates OAuth2 authentication flow.
    /// Redirects user to Etsy login page.
    /// </summary>
    private static IResult LoginAsync(HttpContext context)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/" // Redirect to home after login
        };

        return Results.Challenge(properties, ["Etsy"]);
    }

    /// <summary>
    /// GET /auth/logout
    /// Signs out the user and clears authentication cookies.
    /// </summary>
    private static async Task<IResult> LogoutAsync(HttpContext context)
    {
        await context.SignOutAsync("cookie");
        return Results.Redirect("/");
    }

    /// <summary>
    /// GET /auth/profile
    /// Returns the current authenticated user's profile information.
    /// Available claims: user_id, shop_id, name, email, etc.
    /// </summary>
    private static IResult GetProfileAsync(HttpContext context)
    {
        var user = context.User;

        if (!user.Identity?.IsAuthenticated ?? false)
        {
            return Results.Unauthorized();
        }

        var profile = new
        {
            UserId = user.FindFirst("user_id")?.Value,
            ShopId = user.FindFirst("shop_id")?.Value,
            Name = user.FindFirst(ClaimTypes.Name)?.Value,
            Email = user.FindFirst(ClaimTypes.Email)?.Value,
            Claims = user.Claims
                .Select(c => new { c.Type, c.Value })
                .ToList()
        };

        return Results.Ok(profile);
    }

    /// <summary>
    /// GET /auth/callback/etsy
    /// Handles the OAuth2 callback from Etsy.
    /// This is called automatically by the authentication middleware.
    /// </summary>
    private static async Task<IResult> HandleCallbackAsync(HttpContext context)
    {
        // The authentication middleware handles this automatically.
        // If we get here with no errors, authentication was successful.

        var result = await context.AuthenticateAsync("Etsy");

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        // Sign in with cookie
        await context.SignInAsync("cookie", result.Principal, result.Properties);

        // Redirect to home or client app
        return Results.Redirect("/");
    }
}
