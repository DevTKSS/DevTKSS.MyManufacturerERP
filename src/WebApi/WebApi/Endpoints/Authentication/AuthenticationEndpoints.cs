namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;

public static class AuthenticationEndpoints
{
    public static RouteGroupBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
    {
        // Map the authentication endpoints
        var group = routes.MapGroup("/connect")
                              .WithTags("Authentication");

        group.MapGet("/authorize", Authorize)
                .WithName("Authorize")
                .WithSummary("Authorize user")
                .WithDescription("Authorize a user and return an HTML page with the authorization code")
                .AllowAnonymous();

        group.MapPost("/token", Token)
                .RequireAuthorization()
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

    private static async Task<Results<ContentHttpResult, BadRequest>> Authorize(HttpContext context)
    {
        var request = context.GetOpenIddictServerRequest();
        if (request is null)
            return TypedResults.BadRequest();

        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name, Claims.Role);
        identity.AddClaim(Claims.Subject, "dummy_user_id");
        identity.AddClaim(Claims.Name, "Test User");

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email);

        await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);

        context.Response.Clear();

        var template = await File.ReadAllTextAsync("page.html");
        var code = Uri.EscapeDataString(context.GetOpenIddictServerResponse()!.Code!);
        var state = Uri.EscapeDataString(context.GetOpenIddictServerResponse()!.State!);
        var redirect = new UriBuilder(request.RedirectUri!)
        {
            Query = $"code={code}&state={state}"
        }.Uri.ToString();

        var html = string.Format(template, redirect);

        return TypedResults.Content(html, MediaTypeNames.Text.Html, Encoding.UTF8, StatusCodes.Status200OK);
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult, BadRequest>> Token(HttpContext context)
    {
        var request = context.GetOpenIddictServerRequest();
        if (request is null)
            return TypedResults.BadRequest();

        if (request.IsAuthorizationCodeGrantType())
        {
            // Normally you retrieve the principal associated with the code.
            // For simplicity, here you recreate it – in production, check that the code has not
            // already been consumed and perform all necessary validations.
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(Claims.Subject, "dummy_user_id");
            identity.AddClaim(Claims.Name, "Test User");

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
            return TypedResults.Ok();
        }
        else
        {
            // If the grant type is not recognized, trigger a Challenge or
            // return an error.
            await context.ChallengeAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return TypedResults.Unauthorized();
        }
    }

    private static Task<Results<Ok<object>, UnauthorizedHttpResult>> UserInfo(HttpContext context)
    {
        // Check that the request is authenticated
        var user = context.User;

        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            return Task.FromResult<Results<Ok<object>, UnauthorizedHttpResult>>(TypedResults.Unauthorized());
        }

        // Create the object to return. You can include more claims if necessary.
        var userInfo = new
        {
            sub = user.FindFirst(Claims.Subject)?.Value,
            name = user.FindFirst(Claims.Name)?.Value,
            email = user.FindFirst(Claims.Email)?.Value
        };

        return Task.FromResult<Results<Ok<object>, UnauthorizedHttpResult>>(TypedResults.Ok((object)userInfo));
    }
}
