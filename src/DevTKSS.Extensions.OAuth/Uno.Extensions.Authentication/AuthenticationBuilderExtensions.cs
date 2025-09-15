using System.Diagnostics.CodeAnalysis;

namespace Uno.Extensions.Authentication;
internal static class AuthenticationBuilderExtensions
{
    internal static IAuthenticationBuilder AddAuthentication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TAuthProvider, TSettings>(
       this IAuthenticationBuilder builder,
       string name,
       TSettings settings,
       Func<TAuthProvider, TSettings, TAuthProvider> configureProvider)
       where TAuthProvider : class, IAuthenticationProvider
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
                services.AddSingleton<IAuthenticationProvider, TAuthProvider>(serviceProvider =>
                {
                    var auth = serviceProvider.GetRequiredService<TAuthProvider>();


                    return configureProvider(auth, settings);
                });
            });
        return builder;
    }
    public static TBuilder AsBuilder<TBuilder>(this IAuthenticationBuilder authBuilder) where TBuilder : IBuilder, new()
    {
        var hostBuilder = (authBuilder as IBuilder)!.HostBuilder!;
        if (hostBuilder is TBuilder builder)
        {
            return builder;
        }

        return new TBuilder { HostBuilder = hostBuilder };
    }
}
