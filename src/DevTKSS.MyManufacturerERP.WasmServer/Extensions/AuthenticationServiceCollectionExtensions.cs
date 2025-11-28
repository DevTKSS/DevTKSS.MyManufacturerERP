using DevTKSS.MyManufacturerERP.Server.Models;
using DevTKSS.MyManufacturerERP.Server.Validation.Identity;

namespace DevTKSS.MyManufacturerERP.Server.Extensions;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        // Add cookie authentication using
        // <see href="https://learn.microsoft.com/de-de/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0"/>
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/signin";
            options.LogoutPath = "/signout";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        })
        .AddEtsy(options =>
        {
            var etsySection = configuration.GetSection("Authentication").GetSection(EtsyAuthenticationDefaults.DisplayName).Get<EtsyAuthenticationOptions>();
            if (etsySection is not null)
            {
                options.ClientId = etsySection.ClientId;
                options.ClientSecret = etsySection.ClientSecret;
                if (etsySection.CallbackPath != default)
                {
                    options.CallbackPath = etsySection.CallbackPath;
                }
                options.IncludeDetailedUserInfo = etsySection.IncludeDetailedUserInfo;

                // Save Tokens and UsePkce is already default in the Provider

                foreach (var scope in etsySection.Scope.Where(sectionScope => !options.Scope.Contains(sectionScope)))
                {
                    options.Scope.Add(scope);
                }
            }

        });

        return services;
    }
    public static IServiceCollection AddIdentityApi(this IServiceCollection services)
    {
        // Identity
        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
        {
           options.Password.RequiredLength = 12;
        })
        .AddSignInManager()
        .AddDefaultTokenProviders()
        .AddRoles<IdentityRole>()
        .AddUserConfirmation<DefaultUserConfirmation<ApplicationUser>>()
        .AddUserManager<UserManager<ApplicationUser>>()
        .AddPasswordValidator<CustomPasswordValidator<ApplicationUser>>()
        .AddUserValidator<CustomUserValidator<ApplicationUser>>()
        .AddErrorDescriber<IdentityErrorDescriber>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }
}
