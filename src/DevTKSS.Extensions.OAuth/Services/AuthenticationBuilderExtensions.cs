using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevTKSS.Extensions.OAuth.Services;

internal static class AuthenticationBuilderExtensions
{
    public static TBuilder AsBuilder<TBuilder>(this IAuthenticationBuilder authBuilder) where TBuilder : IBuilder, new()
    {
        var hostBuilder = (authBuilder as IBuilder)!.HostBuilder!;
        if (hostBuilder is TBuilder builder)
        {
            return builder;
        }

        return new TBuilder { HostBuilder = hostBuilder };
    }

    /// <summary>
    /// Registers desktop loopback OAuth services (System browser + HttpListener broker) into the host while using Uno.Extensions Authentication.
    /// Call this within UseAuthentication(...).
    /// </summary>
    public static IAuthenticationBuilder AddOAuth(this IAuthenticationBuilder builder,
        Action<OAuthOptionsBuilder>? configureOptions = null,
        string name = OAuthProvider.DefaultName)
    {
        var hostBuilder = (builder as IBuilder)?.HostBuilder;
        if (hostBuilder is null)
        {
            return builder;
        }

        // Build options from builder
        var optBuilder = OAuthOptionsBuilder.Create();
        configureOptions?.Invoke(optBuilder);
        var options = optBuilder.Build();

        // Bind options into configuration and add validation
        hostBuilder.UseConfiguration(configure: configBuilder =>
            {
                configBuilder.Section<OAuthOptions>(name);
                // Only bind the section if its valid (ClientID is required)
                if (!string.IsNullOrWhiteSpace(options.ClientID))
                    configBuilder.WithConfigurationSectionFromEntity(options, name);
                return configBuilder;
            });

        hostBuilder.ConfigureServices(services =>
        {

            services.AddOAuthServices(name);

        });

        return builder;
    }

}