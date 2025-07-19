namespace DevTKSS.MyManufacturerERP.Server.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        // Add cookie authentication using
        // <see href="https://learn.microsoft.com/de-de/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0"/>
        .AddCookie()//options=>
                    // {
                    // Configure forwarding the authentication using
                    // <see href="https://learn.microsoft.com/de-de/aspnet/core/security/authentication/policyschemes?view=aspnetcore-9.0/>
                    // For example, can foward any requests that start with /api 
                    // to the api scheme.

        //options.ForwardDefaultSelector = ctx =>
        //    ctx.Request.Path.StartsWithSegments("/api") ? "Api" : null;
        // }).AddYourAuthenticationScheme("Api")
        .AddOAuth("Etsy", options =>
        {
            var etsyConfig = configuration.GetSection("Authentication")
                                                                    .GetSection("Etsy");

            options.ClientId = etsyConfig.GetValue<string>("ClientId") ?? throw new ArgumentNullException(nameof(options.ClientId));
            options.ClientSecret = etsyConfig.GetValue<string>("ClientSecret") ?? throw new ArgumentNullException(nameof(options.ClientSecret));
            options.CallbackPath = etsyConfig.GetValue<PathString>("CallbackPath", "/etsy-auth-callback");
            options.AuthorizationEndpoint = etsyConfig.GetValue("AuthorizationEndpoint", "https://www.etsy.com/oauth/connect");
            options.TokenEndpoint = etsyConfig.GetValue("TokenEndpoint", "https://openapi.etsy.com/v3/public/oauth/token");
            options.UserInformationEndpoint = etsyConfig.GetValue("UserInformationEndpoint", "https://openapi.etsy.com/v3/application/users/me");
            options.AdditionalAuthorizationParameters.TryAdd("response_type", "code");
            options.AdditionalAuthorizationParameters.TryAdd("grant_type", "authorization_code");

            options.Events = new OAuthEvents
            {
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/Home/Error?message=" + context.Failure?.Message);
                    context.HandleResponse();
                    return Task.CompletedTask;
                },
                OnCreatingTicket = context =>
                {
                    // No Idea how to get this set up properly to do the challenge in the end


                    // Here you can add custom logic to handle the user information
                    // For example, you can fetch user details from the UserInformationEndpoint
                    // and add claims to the identity.
                    return Task.CompletedTask;
                }
            };
            var scopes = etsyConfig.GetSection("Authentication:Etsy")
                                    .GetValue("Scope", string.Empty)
                                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var scope in scopes)
            {
                options.Scope.Add(scope);
            }

            options.SaveTokens = true;
            options.Validate();
            // Optional: Map user claims etc.
        });
        return services;
    }
    public static IApplicationBuilder UseAuthServices(this IApplicationBuilder app)
    {
        // Use authentication middleware
        app.UseAuthentication();
        // Use authorization middleware
        app.UseAuthorization();
        return app;
    }
}