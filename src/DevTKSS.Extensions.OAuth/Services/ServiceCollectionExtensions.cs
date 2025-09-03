using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevTKSS.Extensions.OAuth.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOAuthServices(this IServiceCollection services,string providerName)
    {
        services
            .AddLogging()
            .AddSingleton<IBrowserProvider, BrowserProvider>()
            .AddSingleton<IHttpListenerService, HttpListenerService>()
            .AddSingleton<ISystemBrowserAuthBrokerProvider, SystemBrowserAuthBroker>()
            .AddTransient<IHttpListenerCallbackHandler, AuthCallbackHandler>();
        
        services.TryAddSingleton<OAuthSettings>();
        services.AddTransient<OAuthProvider>()
            .AddSingleton<IAuthenticationService,OAuthAuthenticationService>();

        return services;
    }
    internal static IAuthenticationBuilder AddAuthentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] ToAuthProvider, TSettings>(
        this IAuthenticationBuilder builder,
        string name,
        TSettings settings,
        Func<ToAuthProvider, TSettings, ToAuthProvider> configureProvider)
        where ToAuthProvider : class, IOAuthProvider
        where TSettings : class
    {
        var hostBuilder = (builder as IBuilder)?.HostBuilder;
        if (hostBuilder is null)
        {
            return builder;
        }

        hostBuilder
            .ConfigureServices(services =>
            {
                services.AddSingleton<IOAuthProvider, ToAuthProvider>(serviceProvider =>
                {
                    var auth = serviceProvider.GetRequiredService<ToAuthProvider>();
                   

                    return configureProvider(auth, settings);
                });
            });
        return builder;
    }
}
