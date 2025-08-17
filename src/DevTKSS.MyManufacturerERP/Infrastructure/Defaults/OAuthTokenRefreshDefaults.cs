namespace DevTKSS.MyManufacturerERP.Infrastructure.Defaults;
/// <summary>
/// oAuth Token Request standarized keys from <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.3">4.1.3 Access Token Request</see>
/// </summary>
public class OAuthTokenRefreshDefaults
{
    public const string GrantTypeKey = "grant_type";
    public const string AuthorizationCode = "authorization_code";
    public const string AccessTokenKey = "access_token";
    public const string TokenTypeKey = "token_type";
    public const string RefreshToken = "refresh_token";
    public const string ExpiresInKey = "expires_in";
    public const string ScopeKey = "scope";
}
