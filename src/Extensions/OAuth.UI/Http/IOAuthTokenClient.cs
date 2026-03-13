namespace DevTKSS.Extensions.OAuth.UI.Http;

public interface IOAuthTokenClient
{
    Task<WebAuthRequest?> GetAuthRequestAsync(IDictionary<string, string>? extraParameters = null);
    ValueTask<TokenResponse?> ExchangeCodeAsync(AccessTokenRequest request, CancellationToken cancellationToken = default);
    ValueTask<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
