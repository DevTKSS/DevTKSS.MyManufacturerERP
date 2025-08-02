using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;
internal record OAuthSettings
{
    public AsyncFunc<IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? LoginCallback { get; init; }
    public AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?>? RefreshCallback { get; init; }
   
    // there is no logout in OAuth2
    //  public AsyncFunc<IServiceProvider, IDispatcher?, ITokenCache, IDictionary<string, string>, bool>? LogoutCallback { get; init; }
}
