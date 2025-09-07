using Uno.AuthenticationBroker; // Known to cause issue: https://github.com/unoplatform/uno/issues/21237
using Yllibed.HttpServer.Extensions;

namespace DevTKSS.Extensions.OAuth.Browser;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemBrowserServices(this IServiceCollection services, string? authCallbackName = null)
    {
        services
            .AddLogging()
            .AddSingleton<IBrowserProvider, BrowserProvider>();
        
        services.AddYllibedHttpServer()
                /*.AddSingleton<IWebAuthenticationBrokerProvider, SystemBrowserAuthBroker>()*/ // BUG: Known to cause issue: https://github.com/unoplatform/uno/issues/21237
                ;
        if (authCallbackName is not null)
        {
            services
                .AddOAuthCallbackHandler(authCallbackName);
        }
        else
        {
            services
                .AddOAuthCallbackHandler();
        }

        
        return services;
    }

   
   
   
}
