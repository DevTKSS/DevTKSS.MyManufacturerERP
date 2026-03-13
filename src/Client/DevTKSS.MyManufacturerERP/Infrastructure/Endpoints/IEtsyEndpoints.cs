namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;

[Headers($"Content-Type: application/json")]
public interface IEtsyEndpoints
{

    /// <summary>
    /// Authorization ResponseValueCode Request as per OAuthDefaults 2.0 specification. <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.1">4.1.1 Authorization Request</see>
    /// </summary>
    /// <returns>The <see cref="AccessGrantResponse"/></returns>
    [Get("/oauth/connect")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    [Obsolete("OAuth2 Requires Interactive Flow - Use Web Browser to Navigate to the URL")]
    Task<ApiResponse<AccessGrantResponse>> AuthenticateAsync(AuthorizationCodeRequest authorizationCodeRequest, CancellationToken cancellationToken = default);

    [Post("/public/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> ExchangeCodeAsync([Body(BodySerializationMethod.UrlEncoded)] AccessTokenRequest accessTokenRequest, CancellationToken cancellationToken = default);

    [Post("/public/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> RefreshTokenAsync([Body(BodySerializationMethod.UrlEncoded)] RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default);

    // Get current authenticated user (minimal info, requires shops_r scope)
    [Get("/v3/application/users/me")]
    Task<UserMeResponse> GetMeAsync(
        [Authorize("Bearer")] string bearerToken,
        [Header("x-api-key")] string apiKey,
        CancellationToken cancellationToken = default);

    // Get user details (requires email_r scope, user_id must be int64 >= 1)
    [Get("/v3/application/users/{user_id}")]
    Task<UserDetailsResponse> GetUserAsync(
        long user_id,
        [Authorize(scheme: "Bearer")] string bearerToken,
        [Header("x-api-key")] string apiKey,
        CancellationToken cancellationToken = default);
}
