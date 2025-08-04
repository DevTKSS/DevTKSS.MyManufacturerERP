using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTKSS.MyManufacturerERP.Extensions;
public record OAuthAuthenticationSettings
{
    public AsyncFunc<IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? LoginCallback { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
    public AsyncFunc<IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, bool>? LogoutCallback { get; init; }
}


public record OAuthAuthenticationSettings<TService>
{
    public AsyncFunc<TService, IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? LoginCallback { get; init; }
    public AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
    public AsyncFunc<TService, IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, bool>? LogoutCallback { get; init; }
}
