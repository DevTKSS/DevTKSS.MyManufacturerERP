using Microsoft.AspNetCore.Mvc;

namespace DevTKSS.MyManufacturerERP.Server.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        // Add cookie authentication using
        // <see href="https://learn.microsoft.com/de-de/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0"/>
        .AddCookie(options =>
        {
            // Configure the cookie authentication options
            // <see href="https://learn.microsoft.com/de-de/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0"/>
            options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            options.SlidingExpiration = true;
            options.AccessDeniedPath = "/Forbidden/";
        })
        .AddOAuth("Etsy", options =>
        {
            var etsyConfig = configuration.GetSection("Authentication").GetSection("Etsy");
            options.ClientId = etsyConfig["ClientId"] ?? throw new ArgumentNullException(options.ClientId);
            options.ClientSecret = etsyConfig["ClientSecret"] ?? throw new ArgumentNullException(options.ClientSecret);
            options.CallbackPath = etsyConfig.GetValue("CallbackPath", "/etsy-auth-callback");
            options.AuthorizationEndpoint = etsyConfig.GetValue("AuthorizationEndpoint", "https://www.etsy.com/oauth/connect");
            options.TokenEndpoint = etsyConfig.GetValue("TokenEndpoint", "https://openapi.etsy.com/v3/public/oauth/token");
            options.UserInformationEndpoint = etsyConfig.GetValue("UserInformationEndpoint", "https://openapi.etsy.com/v3/application/users/me");
            options.UsePkce = true;
            options.SaveTokens = true;
            options.Scope.Clear();
            var scopes = etsyConfig.GetValue("Scope", string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var scope in scopes)
            {
                options.Scope.Add(scope);
            }
            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    // Get user info from Etsy
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                    var response = await context.Backchannel.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.RootElement.GetProperty("user_id").GetString() ?? ""));
                    context.Identity?.AddClaim(new Claim(ClaimTypes.Name, user.RootElement.GetProperty("login_name").GetString() ?? ""));
                },
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/login?error=" + Uri.EscapeDataString(context.Failure?.Message ?? "OAuth Error"));
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}