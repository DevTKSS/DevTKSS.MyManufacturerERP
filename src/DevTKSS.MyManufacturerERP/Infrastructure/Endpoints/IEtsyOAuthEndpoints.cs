using DevTKSS.Extensions.OAuth.Defaults;
using DevTKSS.Extensions.OAuth.Responses;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;
[Headers($"Content-Type: application/json")]
public interface IEtsyOAuthEndpoints
{
    /// <summary>
    /// Authorization ResponseValueCode Request as per OAuth 2.0 specification. <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.1">4.1.1 Authorization Request</see>
    /// </summary>
    /// <returns>The <see cref="AccessGrantResponse"/></returns>
    [Get("/oauth/connect")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<ApiResponse<AccessGrantResponse>> SendAuthorizationCodeRequestAsync(AuthorizationCodeRequest authorizationCodeRequest);
    [Post("/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> ExchangeCodeAsync([Body(BodySerializationMethod.UrlEncoded)] AccessTokenRequest accessTokenRequest);

    [Post("/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> RefreshTokenAsync([Body(BodySerializationMethod.UrlEncoded)]RefreshTokenRequest refreshTokenRequest);

}
/// <summary>
/// Authorization ResponseValueCode Request as per OAuth 2.0 specification. <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.1">4.1.1 Authorization Request</see>
/// </summary>
public record AuthorizationCodeRequest
{
    /// <summary>
    /// REQUIRED.  Value MUST be set to "code".
    /// </summary>
    [JsonPropertyName(OAuthAuthRequestDefaults.ResponseTypeKey)]
    public string ResponseType { get; set; } = OAuthAuthRequestDefaults.CodeKey;
    /// <summary>
    /// REQUIRED.  The client identifier as described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-2.2"/>Section 2.2
    /// </summary>
    [JsonPropertyName(OAuthAuthRequestDefaults.ClientIdKey)]
    public string ClientId { get; set; } = string.Empty;
    /// <summary>
    /// OPTIONAL.  As described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2"/>Section 3.1.2.
    /// </summary>
    [JsonPropertyName(OAuthAuthRequestDefaults.RedirectUriKey)]
    public string RedirectUri { get; set; } = string.Empty;
    /// <summary>
    /// OPTIONAL.  The scope of the access request as described by <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.3"/>Section 3.3.
    /// </summary>
    [JsonPropertyName(OAuthAuthRequestDefaults.ScopeKey)]
    public string Scope { get; set; } = string.Empty;
    /// <summary>
    /// RECOMMENDED.  An opaque value used by the client to maintain state between the request and callback.
    /// The authorization server includes this value when redirecting the user-agent back to the client.The parameter SHOULD be used for preventing
    /// cross-site request forgery as described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-10.12"/>Section 10.12
    /// </summary>
    [JsonPropertyName(OAuthAuthRequestDefaults.StateKey)]
    public string State { get; set; } = string.Empty;
    [JsonPropertyName(OAuthPkceDefaults.CodeChallengeKey)]
    public string CodeChallenge { get; set; } = string.Empty;
    [JsonPropertyName(OAuthPkceDefaults.CodeChallengeMethodS256)]
    public string CodeChallengeMethod { get; set; } = OAuthPkceDefaults.CodeChallengeMethodS256;
}
public record AccessTokenRequest
{
    [JsonPropertyName(OAuthTokenRefreshDefaults.GrantTypeKey)]
    public string GrantType { get; set; } = OAuthTokenRefreshDefaults.RefreshToken;

    [JsonPropertyName(OAuthAuthRequestDefaults.ClientIdKey)]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName(OAuthAuthRequestDefaults.RedirectUriKey)]
    public string RedirectUri { get; set; } = string.Empty;

    [JsonPropertyName(OAuthAuthRequestDefaults.CodeKey)]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName(OAuthPkceDefaults.CodeVerifierKey)]
    public string CodeVerifier { get; set; } = string.Empty;
}
public record RefreshTokenRequest
{
    [JsonPropertyName(OAuthTokenRefreshDefaults.GrantTypeKey)]
    public string GrantType { get; set; } = OAuthTokenRefreshDefaults.AuthorizationCode;
    [JsonPropertyName(OAuthAuthRequestDefaults.ClientIdKey)]
    public string ClientId { get; set; } = string.Empty;
    [JsonPropertyName(OAuthTokenRefreshDefaults.RefreshToken)]
    public string RefreshToken { get; set; } = string.Empty;
}