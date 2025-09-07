
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevTKSS.Extensions.OAuth.Services;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOAuthServices(this IServiceCollection services, string providerName) // TODO: Should in the end be similar like WebAuthenticationProvider with added OAuth functionallity
    {
        services.AddSingleton<IAuthenticationService, AuthenticationService>();// TODO: Check if we need more services or options imports
        services.TryAddSingleton<OAuthSettings>();
        services.AddTransient<IAuthProvider, OAuthProvider>();
        return services;
    }
}
