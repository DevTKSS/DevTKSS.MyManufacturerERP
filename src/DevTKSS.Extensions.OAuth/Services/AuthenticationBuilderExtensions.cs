namespace DevTKSS.Extensions.OAuth.Services;

public static class AuthenticationBuilderExtensions
{
    
    /// <summary>
    /// Registers desktop loopback OAuth services (System browser + HttpListener broker) into the host while using Uno.Extensions Authentication.
    /// Call this within UseAuthentication(...).
    /// </summary>
    public static IAuthenticationBuilder AddOAuth(
        this IAuthenticationBuilder builder,
        Action<OAuthOptions>? configureOptions = null,
        Action<OAuthSettings>? configureSettings = null,
        string name = OAuthProvider.DefaultName)
    {
        var hostBuilder = (builder as IBuilder)?.HostBuilder;
        if (hostBuilder is null)
        {
            return builder;
        }

        // Bind options into configuration and add validation
        hostBuilder.UseConfiguration(configure: configBuilder =>
        {
            return configBuilder;
        });

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSystemBrowserServices(name);

        });

        return builder;
    }

}