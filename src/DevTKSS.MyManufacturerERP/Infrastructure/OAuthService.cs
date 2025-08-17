namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;
public class OAuthService : IAuthenticationService
{

    public OAuthService()
    {
    }
    public string[] Providers => new[] { "EtsyOAuth" };

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