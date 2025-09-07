namespace DevTKSS.Extensions.OAuth.Options;

/// <summary>
/// Represents configuration options for token keys used in authentication or authorization workflows.<br/>
/// The Token Keys will be used to store and retrieve tokens from a cache or storage mechanism.
/// </summary>
/// <remarks>
/// This class provides predefined keys for common token types, such as access tokens, refresh tokens, 
/// and ID tokens, while also allowing the inclusion of additional custom token keys through the  <see
/// cref="OtherTokenKeys"/> property.
/// </remarks>
public class TokenCacheKeyOptions
{
    public string AccessTokenKey { get; init; } = "AccessToken";
    public string RefreshTokenKey { get; init; } = "RefreshToken";
    public string ExpirationDateKey { get; init; } = "ExpirationDate";
    public string IdTokenKey { get; init; } = "UserId";
    public IDictionary<string, string> OtherTokenKeys { get; init; } = new Dictionary<string, string>();

}