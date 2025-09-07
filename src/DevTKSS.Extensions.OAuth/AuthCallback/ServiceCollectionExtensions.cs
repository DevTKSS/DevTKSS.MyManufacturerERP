namespace DevTKSS.Extensions.OAuth.AuthCallback;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOAuthCallbackHandler(this IServiceCollection services, string name)
    {
        services.AddSingleton<IAuthCallbackHandler, OAuthCallbackHandler>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsSnapshot<AuthCallbackOptions>>().Get(name); // regular IOptions do not support named options
            return new OAuthCallbackHandler(options);
        });
        return services;
    }
    public static IServiceCollection AddOAuthCallbackHandler(this IServiceCollection services)
    {
        services.AddSingleton<IAuthCallbackHandler, OAuthCallbackHandler>(sp =>
            new OAuthCallbackHandler(sp.GetRequiredService<IOptions<AuthCallbackOptions>>()));
        return services;
    }
    public static IServiceCollection AddAuthCallbackHandler(this IServiceCollection services, Action<AuthCallbackOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IAuthCallbackHandler, OAuthCallbackHandler>();
        return services;
    }
    public static IServiceCollection AddAuthCallbackHandler<TCallbackHandler>(this IServiceCollection services)
        where TCallbackHandler : class, IAuthCallbackHandler
    {
        services.AddSingleton<IAuthCallbackHandler, TCallbackHandler>();
        return services;
    }
    public static IServiceCollection AddAuthCallbackHandler<TService>(this IServiceCollection services, Action<AuthCallbackOptions> configureOptions)
        where TService : class, IAuthCallbackHandler
    {
        services.Configure(configureOptions);
        services.AddSingleton<IAuthCallbackHandler, TService>();
        return services;
    }
    public static IServiceCollection AddOAuthCallbackHandlerAndRegister<TService>(this IServiceCollection services, string name = OAuthCallbackHandler.DefaultName, Action<AuthCallbackOptions>? configureOptions = null)
    where TService : class, IAuthCallbackHandler
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        services.AddOAuthCallbackHandler(name);
        // Register a singleton that wires the handler into the server on construction
        services.AddSingleton<OAuthCallbackHandlerRegistration>();
        return services;
    }
    private sealed class OAuthCallbackHandlerRegistration : IDisposable
    {
        private readonly IDisposable _registration;
        public OAuthCallbackHandlerRegistration(Server server, OAuthCallbackHandler handler) =>
            // Place first by registering now; Server keeps order of registration
            _registration = server.RegisterHandler(handler);
        public void Dispose() => _registration.Dispose();
    }
}
