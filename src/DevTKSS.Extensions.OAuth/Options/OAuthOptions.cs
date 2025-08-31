namespace DevTKSS.Extensions.OAuth.Options;
public class OAuthEndpointOptions 
{
    public const string ConfigurationSection = "EndpointOptions";

    public string? AuthorizationEndpoint { get; init; }
    public string? UserInfoEndpoint { get; init; }
    public string? TokenEndpoint { get; init; }
    public string? RedirectUri { get; init; }
}

public class OAuthOptions : EndpointOptions
{
    public const string DefaultName = "OAuth";

    /// <summary>
    /// Gets the unique identifier for the client. Can be named as keystring at registration.
    /// </summary>
    public string? ClientID { get; init; }
    public string? ClientSecret { get; init; }
    public string ProviderName { get; init; } = DefaultName;
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? ExpirationDate { get; init; }
    public string? IdToken { get; init; }
    public string[] Scopes { get; init; } = []; 
    public IDictionary<string, string> AdditionalParameters { get; init; } = new Dictionary<string,string> ();
    public OAuthEndpointOptions EndpointOptions { get; init; } = new ();
    public TokenCacheKeyOptions TokenCacheKeyOptions { get; init; } = new ();
}
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