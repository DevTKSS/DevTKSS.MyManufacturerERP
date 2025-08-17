using DevTKSS.MyManufacturerERP.WebApi.Contracts;

namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;

public static class AuthenticationEndpoints
{
    public static RouteGroupBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/connect")
                          .WithTags("Authentication");

        group.MapGet("/authorize", Authorize)
            .WithName("Authorize")
            .WithSummary("Authorize user")
            .WithDescription("Authorize a user and return an HTML page with the authorization code")
            .AllowAnonymous();

        group.MapPost("/token", Token)
            .AllowAnonymous()
            .WithName("Token")
            .WithSummary("Exchange authorization code for access token")
            .WithDescription("Exchange an authorization code for an access token");

        group.MapGet("/userinfo", UserInfo)
            .RequireAuthorization()
            .WithName("UserInfo")
            .WithSummary("Get user information")
            .WithDescription("Retrieve user information based on the authenticated user's claims");

        return group;
    }

    // Simulate the OAuth2 Authorization Endpoint
    private static IResult Authorize(HttpContext context)
    {
        // TODO: Validate client_id, redirect_uri, scope, state, etc.
        var query = context.Request.Query;
        var clientId = query["client_id"].ToString();
        var redirectUri = query["redirect_uri"].ToString();
        var state = query["state"].ToString();

        // Simulate user login and consent
        var code = Guid.NewGuid().ToString("N"); // In production, store and associate with user/session

        // Build redirect URI with code and state
        var uriBuilder = new UriBuilder(redirectUri);
        var parameters = $"code={code}&state={state}";
        uriBuilder.Query = parameters;

        // Redirect back to client
        return Results.Redirect(uriBuilder.ToString());
    }

    // Simulate the OAuth2 Token Endpoint
    private static async Task<Results<Ok<object>, BadRequest>> Token(HttpContext context)
    {
        // Parse the request body as AccessTokenRequest
        var request = await context.Request.ReadFromJsonAsync<AccessTokenRequest>();
        if (request == null || string.IsNullOrEmpty(request.Code))
            return TypedResults.BadRequest();

        // TODO: Validate code, client_id, redirect_uri, code_verifier, etc.

        // Simulate token response
        var tokenResponse = new
        {
            access_token = Guid.NewGuid().ToString("N"),
            token_type = "Bearer",
            expires_in = 3600,
            refresh_token = Guid.NewGuid().ToString("N"),
            scope = request.Scope
        };

        return TypedResults.Ok((object)tokenResponse);
    }

    // Simulate the OAuth2 UserInfo Endpoint
    private static Task<Results<Ok<object>, UnauthorizedHttpResult>> UserInfo(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult<Results<Ok<object>, UnauthorizedHttpResult>>(TypedResults.Unauthorized());

        var userInfo = new
        {
            sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "dummy_user_id",
            name = user.FindFirst(ClaimTypes.Name)?.Value ?? "Test User",
            email = user.FindFirst(ClaimTypes.Email)?.Value ?? "test@example.com"
        };

        return Task.FromResult<Results<Ok<object>, UnauthorizedHttpResult>>(TypedResults.Ok((object)userInfo));
    }
}
