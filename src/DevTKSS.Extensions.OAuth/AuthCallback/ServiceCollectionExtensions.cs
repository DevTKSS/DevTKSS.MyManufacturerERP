namespace DevTKSS.Extensions.OAuth.AuthCallback;
public static class ServiceCollectionExtensions
{
	#region Non-generic overloads
	public static IServiceCollection AddOAuthCallbackHandler(this IServiceCollection services) // simplest implementation by factory
	{
		services.AddSingleton<IAuthCallbackHandler, OAuthCallbackHandler>(sp =>
			new OAuthCallbackHandler(sp.GetRequiredService<IOptions<AuthCallbackOptions>>()));

		services.AddSingleton<IHttpHandler>(sp =>
			sp.GetRequiredService<IAuthCallbackHandler>());

		return services;
	}

	public static IServiceCollection AddOAuthCallbackHandler(
		this IServiceCollection services,
		Action<AuthCallbackOptions> configureOptions)
	{
		services.Configure(configureOptions);
		services.AddOAuthCallbackHandler();
	   
		return services;
	}

	#region Keyed overload
	//public static IServiceCollection AddKeyedOAuthCallbackHandler(this IServiceCollection services, string name) // named implementation
	//{
	//	services.AddKeyedSingleton<IAuthCallbackHandler, OAuthCallbackHandler>(name,(sp,_) =>
	//	   new OAuthCallbackHandler(sp.GetRequiredService<IOptionsSnapshot<AuthCallbackOptions>>().Get(name)));// regular IOptions do not support named options
	//	services.AddSingleton<IHttpHandler>(sp => sp.GetRequiredKeyedService<IAuthCallbackHandler>(name));
		   
	//	return services;
	//}

	//public static IServiceCollection AddKeyedOAuthCallbackHandler(this IServiceCollection services, Action<AuthCallbackOptions> configureOptions, string name)
	//{
	//	services.Configure(name, configureOptions);
	//	services.AddKeyedOAuthCallbackHandler(name);
	//	return services;
	//}

	#endregion
	public static IServiceCollection AddOAuthCallbackHandlerAndRegister(
	   this IServiceCollection services,
	   Action<AuthCallbackOptions>? configureOptions = null,
	   string? name = null)
	{
		if (configureOptions is not null)
		{
			//if (name is not null)
			//{
			//	services.AddKeyedOAuthCallbackHandler(configureOptions, name);
			//}
			//else
			//{
				services.AddOAuthCallbackHandler(configureOptions);
			//}
		}
		//else if (name is not null)
		//{
		//	services.AddKeyedOAuthCallbackHandler(name);
		//}
		else
		{
			services.AddOAuthCallbackHandler();
		}

		// Register a singleton that wires the handler into the server on construction
		services.AddSingleton<OAuthCallbackHandlerRegistration>();
		return services;
	}
	#endregion

	#region Generic overloads
	public static IServiceCollection AddAuthCallbackHandler<TCallbackHandler, TOptions>(
	this IServiceCollection services,
	Action<TOptions> configureOptions)
	where TCallbackHandler : class, IAuthCallbackHandler
	where TOptions : AuthCallbackOptions, new()
	{
		services.Configure<TOptions>(configureOptions);
		services.AddAuthCallbackHandler<TCallbackHandler>();
		return services;
	}
	public static IServiceCollection AddAuthCallbackHandler<TCallbackHandler>(this IServiceCollection services)
		where TCallbackHandler : class, IAuthCallbackHandler
	{
		services.AddSingleton<IAuthCallbackHandler, TCallbackHandler>();

		services.AddSingleton<IHttpHandler>(sp => sp.GetRequiredService<TCallbackHandler>());

		return services;
	}
#region Keyed overload
	//public static IServiceCollection AddKeyedAuthCallbackHandler<TCallbackHandler, TOptions>(this IServiceCollection services, Action<TOptions> configure, string name)
 //  where TCallbackHandler : class, IAuthCallbackHandler
 //  where TOptions : AuthCallbackOptions, new()
	//{
	//	services.Configure(name, configure);
	//	services.AddKeyedAuthCallbackHandler<TCallbackHandler>(name);
	//	return services;
	//}
	//public static IServiceCollection AddKeyedAuthCallbackHandler<TCallbackHandler>(this IServiceCollection services, string name) // named implementation
	//where TCallbackHandler : class, IAuthCallbackHandler
	//{
	//	services.AddKeyedSingleton<IAuthCallbackHandler, TCallbackHandler>(name);// regular IOptions do not support named options
	//	services.AddSingleton<IHttpHandler>(sp =>
	//		sp.GetRequiredKeyedService<IAuthCallbackHandler>(name));

	//	return services;
	//}
	#endregion
	public static IServiceCollection AddAuthCallbackHandlerAndRegister<TCallbackHandler, TOptions>(
	   this IServiceCollection services,
	   Action<TOptions>? configure = null,
	   string? name = null)
	   where TCallbackHandler : class, IAuthCallbackHandler
	   where TOptions : AuthCallbackOptions, new()
	{
		if (configure is not null)
		{
			//if (name is not null)
			//{
			//	services.AddKeyedAuthCallbackHandler<TCallbackHandler, TOptions>(configure, name);
			//}
			//else
			//{
				services.AddAuthCallbackHandler<TCallbackHandler, TOptions>(configure);
			//}
		}
		//else if (name is not null)
		//{
		//	services.AddKeyedAuthCallbackHandler<TCallbackHandler>(name);
		//}
		//else
		//{
			services.AddAuthCallbackHandler<TCallbackHandler>();
		//}

		services.AddSingleton<AuthCallbackHandlerRegistration>();
		return services;
	}
	#endregion

	private sealed class OAuthCallbackHandlerRegistration : IDisposable
	{
		private readonly IDisposable _registration;
		public OAuthCallbackHandlerRegistration(Server server, OAuthCallbackHandler handler) =>
			// Place first by registering now; Server keeps order of registration
			_registration = server.RegisterHandler(handler);

		public void Dispose() => _registration.Dispose();
	}
	private sealed class AuthCallbackHandlerRegistration : IDisposable
	{
		private readonly IDisposable _registration;
		public AuthCallbackHandlerRegistration(Server server, IAuthCallbackHandler handler) =>
			// Place first by registering now; Server keeps order of registration
			_registration = server.RegisterHandler(handler);
		public void Dispose() => _registration.Dispose();
	}
}
