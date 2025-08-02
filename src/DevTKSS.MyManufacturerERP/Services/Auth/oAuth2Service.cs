using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTKSS.MyManufacturerERP.Services.Auth;
internal class oAuth2Service : IAuthenticationService
{
    public string[] Providers => [nameof(OAuth2AuthenticationProvider)];

    public event EventHandler LoggedOut;

    public ValueTask<bool> IsAuthenticated(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> LoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials = null, string? provider = null, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> LogoutAsync(IDispatcher? dispatcher, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> RefreshAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
}
