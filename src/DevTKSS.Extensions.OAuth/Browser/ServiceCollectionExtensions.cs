using DevTKSS.Extensions.OAuth.AuthCallback;
using DevTKSS.Extensions.OAuth.AuthCallbackHandler;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Uno.AuthenticationBroker; // Known to cause issue: https://github.com/unoplatform/uno/issues/21237
using Yllibed.HttpServer.Extensions;

namespace DevTKSS.Extensions.OAuth.Browser;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemBrowserServices(this IServiceCollection services, string name)
    {
        services
            .AddAuthCallbackHandler(name)
            .AddSingleton<IBrowserProvider, BrowserProvider>()
            .AddYllibedHttpServer()
            .AddSingleton<IWebAuthenticationBrokerProvider, SystemBrowserAuthBroker>(); // BUG: Known to cause issue: https://github.com/unoplatform/uno/issues/21237

        return services;
    }

   
   
   
}
