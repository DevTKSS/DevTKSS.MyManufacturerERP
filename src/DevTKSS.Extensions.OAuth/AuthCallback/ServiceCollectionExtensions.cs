namespace DevTKSS.Extensions.OAuth.AuthCallback;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthCallbackHandler(this IServiceCollection services, string name)
    {
        services.AddSingleton<IAuthCallbackHandler, AuthCallbackHandler>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsSnapshot<AuthCallbackOptions>>().Get(name); // regular IOptions do not support named options
            return new AuthCallbackHandler(options);
        };
        return services;
    }
    public static IServiceCollection AddAuthCallbackHandler(this IServiceCollection services)
    {
        services.AddSingleton<IAuthCallbackHandler, AuthCallbackHandler>(sp =>
            new AuthCallbackHandler(sp.GetRequiredService<IOptions<AuthCallbackOptions>>()));
        return services;
    }
    public static IServiceCollection AddAuthCallbackHandler(this IServiceCollection services, Action<AuthCallbackOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IAuthCallbackHandler, AuthCallbackHandler>();
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
}
