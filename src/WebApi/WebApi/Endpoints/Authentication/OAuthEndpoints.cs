using System.Security.Claims;
using DevTKSS.MyManufacturerERP.DataContracts.OAuth;

namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;

/// <summary>
/// OAuth2 authentication endpoints for Etsy integration.
/// Handles login, logout, token exchange, and user profile information.
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

        group.MapPost("/token", (Delegate)ExchangeTokenAsync)
            .WithName("PostOAuthToken")
            .WithSummary("Exchange cookie session for bearer tokens")
            .WithDescription("Returns bearer tokens for an authenticated cookie session. Used by native clients to bridge cookie-based and token-based auth.")
            .RequireAuthorization();

        group.MapGet("/callback/etsy", (Delegate)HandleCallbackAsync)
            .WithName("GetOAuthCallback")
            .WithSummary("OAuth Callback Handler")
            .WithDescription("Handles the redirect from Etsy after user authentication")
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    private static IResult LoginAsync(HttpContext context, string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/auth/callback/etsy",
            Items = { ["returnUrl"] = returnUrl ?? "/" }
        };

        return Results.Challenge(properties, ["Etsy"]);
    }

    private static async Task<IResult> LogoutAsync(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/");
    }

    private static IResult GetProfileAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var profile = new UserProfileResponse
        {
            UserId = user.FindFirst("user_id")?.Value,
            ShopId = user.FindFirst("shop_id")?.Value,
            PrimaryEmail = user.FindFirst(ClaimTypes.Email)?.Value,
            GivenName = user.FindFirst(ClaimTypes.GivenName)?.Value,
            Surname = user.FindFirst(ClaimTypes.Surname)?.Value,
            Name = user.FindFirst(ClaimTypes.Name)?.Value,
            ProfileImageUrl = user.FindFirst("picture")?.Value ?? user.FindFirst("image_75x75_url")?.Value,
            Subject = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Claims = user.Claims
                .Select(c => new ClaimInfo { Type = c.Type, Value = c.Value })
                .ToList()
        };

        return Results.Ok(profile);
    }

    /// <summary>
    /// POST /auth/token
    /// Bridge endpoint: converts a cookie-authenticated session into bearer tokens
    /// that native/desktop clients can use for subsequent API calls.
    /// </summary>
    private static async Task<IResult> ExchangeTokenAsync(HttpContext context)
    {
        var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded || result.Principal is null)
        {
            return Results.Unauthorized();
        }

        var accessToken = result.Properties?.GetTokenValue("access_token");
        var refreshToken = result.Properties?.GetTokenValue("refresh_token");
        var expiresAt = result.Properties?.GetTokenValue("expires_at");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Results.Problem(
                detail: "No access token found in the authentication session. Ensure the OAuth provider stores tokens.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        var tokenResponse = new
        {
            access_token = accessToken,
            refresh_token = refreshToken ?? string.Empty,
            token_type = "Bearer",
            expires_at = expiresAt ?? string.Empty,
            user_id = result.Principal.FindFirst("user_id")?.Value ?? result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
        };

        return Results.Ok(tokenResponse);
    }

    /// <summary>
    /// GET /auth/callback/etsy
    /// Handles the OAuth2 callback from Etsy, signs in via cookie, and
    /// redirects back to the client with a session indicator.
    /// </summary>
    private static async Task<IResult> HandleCallbackAsync(HttpContext context)
    {
        var result = await context.AuthenticateAsync("Etsy");

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        var cookieOptions = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        if (result.Properties is not null)
        {
            cookieOptions.StoreTokens(result.Properties.GetTokens());
        }

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            result.Principal!,
            cookieOptions);

        var returnUrl = result.Properties?.Items["returnUrl"] ?? "/";
        return Results.Redirect(returnUrl);
    }
}
