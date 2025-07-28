namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;

//public static class AuthenticationEndpoints
//{
//    public static RouteGroupBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
//    {
//        // Map the authentication endpoints
//        var connectgroup = routes.MapGroup("/connect")
//                              .WithTags("Authentication");

//        connectgroup.MapGet("/authorize", Authorize)
//            .AllowAnonymous();


//        return connectgroup;
//    }

//    private static async Task<Results<ContentHttpResult,BadRequest>> Authorize(HttpContext context)
//    {
//        var request = context.GetOpenIddictServerRequest();
//        if (request is null)
//            return TypedResults.BadRequest();

//        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name, Claims.Role);
//        identity.AddClaim(Claims.Subject, "dummy_user_id");
//        identity.AddClaim(Claims.Name, "Test User");

//        var principal = new ClaimsPrincipal(identity);
//        principal.SetScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email);

//        await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);

//        context.Response.Clear();

//        var template = await File.ReadAllTextAsync("page.html");
//        var code = Uri.EscapeDataString(context.GetOpenIddictServerResponse()!.Code!);
//        var state = Uri.EscapeDataString(context.GetOpenIddictServerResponse()!.State!);
//        var redirect = new UriBuilder(request.RedirectUri!)
//        {
//            Query = $"code={code}&state={state}"
//        }.Uri.ToString();


//        var html = string.Format(template, redirect);

//        return TypedResults.Content(html,MediaTypeNames.Text.Html,Encoding.UTF8,StatusCodes.Status200OK);

//    }
//}
