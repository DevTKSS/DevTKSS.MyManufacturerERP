
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Uno.Extensions;
public static class HostBuilderExtensions
{
    internal static IAuthenticationBuilder AddAuthentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TAuthenticationProvider, TSettings>(
   this IAuthenticationBuilder builder,
   string name,
   TSettings settings,
   Func<TAuthenticationProvider, TSettings, TAuthenticationProvider> configureProvider)
   where TAuthenticationProvider : class, IAuthenticationProvider
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
                services.TryAddTransient<TAuthenticationProvider>();
                services.AddSingleton<IProviderFactory>(sp =>
                {
                    var auth = sp.GetRequiredService<TAuthenticationProvider>();
                    return new ProviderFactory<TAuthenticationProvider, TSettings>(
                                name,
                                auth,
                                settings,
                                configureProvider);
                });
            });
        return builder;
    }
}