using DevTKSS.MyManufacturerERP.Infrastructure.Defaults;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;
[Headers($"Content-Type: application/json")]
public interface IEtsyOAuthEndpoints
{
    /// <summary>
    /// Authorization ResponseValueCode Request as per OAuth 2.0 specification. <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.1">4.1.1 Authorization Request</see>
    /// </summary>
    /// <param name="responseType">REQUIRED.  Value MUST be set to "code".</param>
    /// <param name="client_id">REQUIRED.  The client identifier as described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-2.2"/>Section 2.2</param>.
    /// <param name="redirectUri">OPTIONAL.  As described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2"/>Section 3.1.2</param>.
    /// <param name="scope">OPTIONAL.  The scope of the access request as described by <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.3"/>Section 3.3.</param>
    /// <param name="state">
    /// RECOMMENDED.  An opaque value used by the client to maintain 
    /// _state between the request and callback.The authorization
    /// server includes this value when redirecting the user-agent back
    /// to the client.The parameter SHOULD be used for preventing
    /// cross-site request forgery as described in <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-10.12"/>Section 10.12</param>.
    /// <param name="codeChallenge"></param>
    /// <param name="codeChallengeMethod"></param>
    /// <returns>The <see cref="AccessGrantResponse"/></returns>
    [Get("/oauth/connect")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<ApiResponse<AccessGrantResponse>> SendAuthorizationCodeRequestAsync(
        [AliasAs(OAuthAuthRequestDefaults.ResponseTypeKey)]string responseType,
        [AliasAs(OAuthAuthRequestDefaults.RedirectUriKey)]string redirectUri,
        [AliasAs(OAuthAuthRequestDefaults.ScopeKey)]string scope,
        [AliasAs(OAuthAuthRequestDefaults.ClientIdKey)]string client_id,
        [AliasAs(OAuthAuthRequestDefaults.StateKey)]string state,
        [AliasAs(OAuthPkceDefaults.CodeChallengeKey)]string codeChallenge,
        [AliasAs(OAuthPkceDefaults.CodeChallengeMethodS256)]string codeChallengeMethod
    );
    [Post("/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> ExchangeCodeAsync(
        [Body(BodySerializationMethod.UrlEncoded)] AccessTokenRequest accessTokenRequest);

    [Post("/oauth/token")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<TokenResponse> RefreshTokenAsync(
        [AliasAs(OAuthTokenRefreshDefaults.GrantTypeKey)] string grantType,
        [AliasAs(OAuthAuthRequestDefaults.ClientIdKey)] string clientId,
        [AliasAs(OAuthTokenRefreshDefaults.RefreshToken)] string refreshToken
    );

}

public class AccessTokenRequest
{
    [JsonPropertyName(OAuthTokenRefreshDefaults.GrantTypeKey)]
    public string GrantType { get; set; } = OAuthTokenRefreshDefaults.GrantTypeAuthorizationCode;

    [JsonPropertyName(OAuthAuthRequestDefaults.ClientIdKey)]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName(OAuthAuthRequestDefaults.RedirectUriKey)]
    public string RedirectUri { get; set; } = string.Empty;

    [JsonPropertyName(OAuthAuthRequestDefaults.ResponseValueCode)]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName(OAuthPkceDefaults.CodeVerifierKey)]
    public string CodeVerifier { get; set; } = string.Empty;
}