namespace DevTKSS.Extensions.OAuth.UI.Http;

public interface IOAuthTokenClient
{
    ValueTask<TokenResponse?> HandleCallbackAsync(Uri redirectUri, CancellationToken cancellationToken = default);

    ValueTask<TokenResponse?> ExchangeCodeAsync(CancellationToken cancellationToken = default);

    ValueTask<TokenResponse?> RefreshTokenAsync(CancellationToken cancellationToken = default);
}
