namespace DevTKSS.Extensions.OAuth.Browser;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IHttpListenerService"/> implementation to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which the service will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOAuth(this IServiceCollection services, string? configurationSection = null)
    {
        if (!string.IsNullOrWhiteSpace(configurationSection))
        {
            services.AddOptionsWithFluentValidation<ServerOptions>(configurationSection);
        }

        services.AddSingleton<IBrowserProvider,BrowserProvider>()
                .AddSingleton<IHttpListenerService, HttpListenerService>();
        return services;
    }
}
