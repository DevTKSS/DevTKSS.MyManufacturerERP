namespace DevTKSS.Extensions.OAuth.OAuthServices;

public static class HostBuilderExtensions
{

	/// <summary>
	/// Registers desktop OAuth services into the host while using Uno.Extensions Authentication.
	/// call this in UseAuthentication of the HostBuilder.
	/// </summary>
	public static IAuthenticationBuilder AddOAuth(
		this IAuthenticationBuilder authBuilder,
		Action<OAuthOptions>? configureOAuthOptions = null,
		Action<AuthCallbackOptions>? configureCallbackOptions = null,
		Action<ServerOptions>? configureServerOptions = null,
		string authKeyName =  OAuthService.DefaultName,
		string callbackKeyName = OAuthCallbackHandler.DefaultName,
		string serverOptionsName = nameof(ServerOptions))
	{
		var hostBuilder = (authBuilder as IBuilder)?.HostBuilder;
		if (hostBuilder is null)
		{
			return authBuilder;
		}
		// Bind options into configuration and add validation
		hostBuilder.UseConfiguration(configure: configBuilder =>
		{
			if (configureOAuthOptions is null)
			{
				configBuilder
					.Section<OAuthOptions>(authKeyName);
			}
			else if (configureCallbackOptions is null)
			{
				configBuilder
					.Section<AuthCallbackOptions>(string.Join(':',authKeyName,callbackKeyName));
			}
			else if (configureServerOptions is null)
			{
				configBuilder
					.Section<ServerOptions>(serverOptionsName);
			}
			return configBuilder;
		});
		hostBuilder.ConfigureServices(services =>
		{
			services.AddSystemBrowserServices(configureCallbackOptions, configureServerOptions, callbackKeyName);
           
            services.AddSingleton<ITokenCache>(sp =>
                new TokenCache(sp.GetRequiredService<ILogger<TokenCache>>(),
                                sp.GetRequiredDefaultInstance<IKeyValueStorage>()))
                    .AddSingleton<IAuthenticationService, AuthenticationService>();

            services.AddSingleton<IOAuthService, OAuthService>();

        });
		authBuilder.AddWeb<IOAuthService>(webAuth=>
		{
			webAuth.PrepareLoginStartUri<IOAuthService>(async (service,serviceProvider,tokenCache,credentials,loginStartUri,cancellationToken) 
				=> await service.PrepareLoginStartUri(loginStartUri,cancellationToken));
			webAuth.PostLogin<IOAuthService>(async (authService, serviceProvider,tokenCache,credentials,redirectUri, tokens,cancellationToken) =>
				await authService.PostLoginAsync(tokens, redirectUri, cancellationToken));
			webAuth.Refresh<IOAuthService>(async (authService, serviceProvider, tokenCache, tokens, cancellationToken) 
				=> await authService.RefreshAsync(cancellationToken));
		},name: authKeyName);

	    

		return authBuilder;
	}

}