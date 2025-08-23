
namespace DevTKSS.MyManufacturerERP.Infrastructure.Services;

public interface IOAuthService
{
    event EventHandler? LoggedOut;

    ValueTask<bool> IsAuthenticated(CancellationToken? cancellationToken = null);
    ValueTask<IDictionary<string, string>?> LoginAsync(IServiceProvider serviceProvider, IDispatcher? dispatcher, ITokenCache tokenCache, IDictionary<string, string> tokens, CancellationToken? cancellationToken = null);
    ValueTask<bool> LogoutAsync(IDispatcher? dispatcher, IDictionary<string, string> tokens, CancellationToken cancellationToken = default);
    ValueTask<IDictionary<string, string>?> RefreshAsync(IServiceProvider serviceProvider, IDictionary<string, string> tokens, CancellationToken? cancellationToken = null);
}