namespace DevTKSS.Extensions.OAuth.Endpoints;
[Headers($"Content-Type: application/json")]
public interface IOAuthEndpoints
{
    /// <summary>
    /// Authorization ResponseValueCode Request as per OAuthDefaults 2.0 specification. <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.1">4.1.1 Authorization Request</see>
    /// </summary>
    /// <returns>The <see cref="AccessGrantResponse"/></returns>
    [Get("/oauth/connect")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<ApiResponse<AccessGrantResponse>> AuthenticateAsync(AuthorizationCodeRequest authorizationCodeRequest,CancellationToken cancellationToken = default);
    [Post("/public/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> ExchangeCodeAsync([Body(BodySerializationMethod.UrlEncoded)] AccessTokenRequest accessTokenRequest, CancellationToken cancellationToken = default);

    [Post("/public/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> RefreshTokenAsync([Body(BodySerializationMethod.UrlEncoded)] RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default);

}
