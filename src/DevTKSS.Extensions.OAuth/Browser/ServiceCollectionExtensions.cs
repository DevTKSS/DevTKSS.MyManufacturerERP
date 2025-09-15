using Uno.AuthenticationBroker; // Known to cause issue: https://github.com/unoplatform/uno/issues/21237
using Yllibed.HttpServer.Extensions;

namespace DevTKSS.Extensions.OAuth.Browser;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSystemBrowserServices(
		this IServiceCollection services,
		Action<AuthCallbackOptions>? configureCallback = null,
		Action<ServerOptions>? configureServer = null,
		string? authCallbackName = null)
	{
		services
			.AddSingleton<IBrowserProvider, BrowserProvider>();
		if(configureServer is not null)
		{
			services.AddYllibedHttpServer(configureServer);
		}else
		{
			services.AddYllibedHttpServer();
		}

		services.AddOAuthCallbackHandlerAndRegister(configureCallback, authCallbackName);
        services.AddSingleton<ISystemBrowserAuthBrokerProvider, SystemBrowserAuthBroker>();

        return services;
	}

	public static IServiceCollection AddSystemBrowserServices<TCallbackHandler,TOptions>(
	 this IServiceCollection services,
	 Action<TOptions>? configureCallback = null,
	 Action<ServerOptions>? configureServer = null,
	 string? authCallbackName = null)
		where TCallbackHandler : class, IAuthCallbackHandler
		where TOptions : AuthCallbackOptions, new()
	{
		services
			.AddSingleton<IBrowserProvider, BrowserProvider>();
		if (configureServer is not null)
		{
			services.AddYllibedHttpServer(configureServer);
		}
		else
		{
			services.AddYllibedHttpServer();
		}
		services.AddAuthCallbackHandlerAndRegister<TCallbackHandler,TOptions>(configureCallback, authCallbackName);
		return services;
	}


}
