/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Exposes extensions allowing to register the OpenIddict client services.
/// </summary>
public static class OpenIddictClientUnoIntegrationExtensions
{
    //https://github.com/Ecierge/openiddict-core/blob/apple-cryptokit/sandbox/OpenIddict.Sandbox.Uno.Client/OpenIddictClientUnoIntegrationExtensions.cs
    // Required until https://github.com/unoplatform/uno.extensions/issues/2381
    /// <summary>
    /// Adds handling of protocol activation
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureDelegate"></param>
    /// <param name="applicationName"></param>
    public static async Task<IApplicationBuilder> UseOpenIddictClientActivationHandlingAsync(this IApplicationBuilder builder, Action<HostBuilderContext, IServiceCollection> configureDelegate, string? applicationName = null)
    {
        // TODO: Check activation here
#if WINDOWS || DESKTOP8_0_OR_GREATER
        var host = new Microsoft.Extensions.Hosting.HostBuilder()
            .ConfigureServices((ctx, services) =>
            {
#if HAS_UNO
                // Required until https://github.com/unoplatform/uno.extensions/issues/2396
                if (!String.IsNullOrEmpty(applicationName))
                {
                    ctx.HostingEnvironment.ApplicationName = applicationName;
                }
                services.AddSingleton(ctx.HostingEnvironment);
#endif
                configureDelegate(ctx, services);
                services.AddSingleton<IHostApplicationLifetime, ActivationHostApplicationLifetime>();
            })
            .Build();
        await host.RunAsync();
        host.Dispose();
#else
        await Task.CompletedTask;
#endif

        return builder.Configure(host =>
            host.ConfigureServices((ctx, services) =>
            {
                // Required until https://github.com/unoplatform/uno.extensions/issues/2396
                if (!String.IsNullOrEmpty(applicationName))
                {
                    ctx.HostingEnvironment.ApplicationName = applicationName;
                }
                services.AddSingleton(ctx.HostingEnvironment);
                configureDelegate(ctx, services);
            }));
    }

    /// <summary>
    /// Registers the OpenIddict client system integration services in the DI container.
    /// </summary>
    /// <param name="builder">The services builder used by OpenIddict to register new services.</param>
    /// <remarks>This extension can be safely called multiple times.</remarks>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public static OpenIddictClientSystemIntegrationBuilder UseUnoIntegration(this OpenIddictClientBuilder builder)
    {
        var systemBuilder = builder.UseSystemIntegration();
        builder.Services.AddSingleton<IHostApplicationLifetime, UnoHostApplicationLifetime>();
        return new OpenIddictClientSystemIntegrationBuilder(builder.Services);
    }

    /// <summary>
    /// Registers the OpenIddict client system integration services in the DI container.
    /// </summary>
    /// <param name="builder">The services builder used by OpenIddict to register new services.</param>
    /// <param name="configuration">The configuration delegate used to configure the client services.</param>
    /// <remarks>This extension can be safely called multiple times.</remarks>
    /// <returns>The <see cref="OpenIddictClientBuilder"/>.</returns>
    public static OpenIddictClientBuilder UseUnoIntegration(
        this OpenIddictClientBuilder builder, Action<OpenIddictClientSystemIntegrationBuilder> configuration)
    {
        builder.UseSystemIntegration(configuration);
        builder.Services.AddSingleton<IHostApplicationLifetime, UnoHostApplicationLifetime>();
        return builder;
    }
}
