namespace DevTKSS.Extensions.OAuth.UI.Http;

public interface IOAuthTokenClient
{
    /// <summary>
    /// Creates a new <see cref="AuthorizationState"/> containing PKCE values and state parameter.
    /// </summary>
    ValueTask<AuthorizationState> GetAuthorizeStateAsync(IDictionary<string, string>? extraParameters = null);

    /// <summary>
    /// Builds a <see cref="WebAuthRequest"/> from the current options and provided authorization state.
    /// </summary>
    ValueTask<WebAuthRequest?> GetWebAuthRequestAsync(AuthorizationState state, IDictionary<string, string>? extraParameters = null);

    /// <summary>
    /// Exchanges the authorization callback result for tokens using the provided state.
    /// </summary>
    ValueTask<TokenResponse?> ExchangeCodeAsync(AuthorizationState state, string callbackResult, CancellationToken cancellationToken = default);

    ValueTask<TokenResponse?> ExchangeCodeAsync(AccessTokenRequest request, CancellationToken cancellationToken = default);
    ValueTask<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
