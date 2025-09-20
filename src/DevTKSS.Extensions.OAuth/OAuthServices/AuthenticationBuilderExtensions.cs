using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevTKSS.Extensions.OAuth.OAuthServices;

public static class HostBuilderExtensions
{

	/// <summary>
	/// Registers desktop OAuth services into the host while using Uno.Extensions Authentication.
	/// call this in UseAuthentication of the HostBuilder.
	/// </summary>
	public static IAuthenticationBuilder AddOAuth(
		this IAuthenticationBuilder builder,
        Action<IOAuthAuthenticationBuilder>? configure = default,
		Action<OAuthOptions>? configureOAuthOptions = null,
		Action<AuthCallbackOptions>? configureCallbackOptions = null,
		Action<ServerOptions>? configureServerOptions = null,
		string authKeyName = OAuthProvider.DefaultName,
		string callbackKeyName = OAuthCallbackHandler.DefaultName,
		string serverOptionsName = nameof(ServerOptions))
	{
		var hostBuilder = (builder as IBuilder)?.HostBuilder;
		if (hostBuilder is null)
		{
			return builder;
		}
		// Bind options into configuration and add validation
		hostBuilder.UseConfiguration(configure: configBuilder =>
		{
			if (configureOAuthOptions is null)
			{
				configBuilder
					.Section<OAuthOptions>(authKeyName);
			}

			return configBuilder;
		});
		hostBuilder.ConfigureServices(services =>
		{
			services.AddSystemBrowserServices(configureCallbackOptions, configureServerOptions, callbackKeyName);

		});
        var authBuilder = builder.AsBuilder<OAuthAuthenticationBuilder>();
        configure?.Invoke(authBuilder);

        return builder.AddAuthentication<OAuthProvider, OAuthSettings>(
            authKeyName,
            authBuilder.Settings,
            (provider,settings) => provider with { Name = authKeyName, Settings = settings });
	}
    /// <summary>
    /// Registers desktop OAuth services into the host while using Uno.Extensions Authentication.
    /// call this in UseAuthentication of the HostBuilder.
    /// </summary>
    public static IAuthenticationBuilder AddOAuth<TService>(
        this IAuthenticationBuilder builder,
        Action<IOAuthAuthenticationBuilder<TService>>? configure = default,
        Action<OAuthOptions>? configureOAuthOptions = null,
        Action<AuthCallbackOptions>? configureCallbackOptions = null,
        Action<ServerOptions>? configureServerOptions = null,
        string authKeyName = OAuthProvider.DefaultName,
        string callbackKeyName = OAuthCallbackHandler.DefaultName,
        string serverOptionsName = nameof(ServerOptions))
        where TService : notnull
    {
        var hostBuilder = (builder as IBuilder)?.HostBuilder;
        if (hostBuilder is null)
        {
            return builder;
        }
        // Bind options into configuration and add validation
        hostBuilder.UseConfiguration(configure: configBuilder =>
        {
            if (configureOAuthOptions is null)
            {
                configBuilder
                    .Section<OAuthOptions>(authKeyName);
            }

            return configBuilder;
        });
        hostBuilder.ConfigureServices(services =>
        {
            services.AddSystemBrowserServices(configureCallbackOptions, configureServerOptions, callbackKeyName);

        });
        var authBuilder = builder.AsBuilder<OAuthAuthenticationBuilder<TService>>();
        configure?.Invoke(authBuilder);

        return builder.AddAuthentication<OAuthProvider<TService>, OAuthSettings<TService>>(
            authKeyName,
            authBuilder.Settings,
            (provider, settings) => provider with { Name = authKeyName, TypedSettings = settings });
    }

}
