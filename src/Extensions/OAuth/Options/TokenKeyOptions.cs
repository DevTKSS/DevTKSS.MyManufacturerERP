namespace DevTKSS.Extensions.OAuth.Options;

public record TokenKeyOptions
{
    public const string ConfigurationSection = "TokenKeys";
    public string IdTokenKey { get; init; } = "IdToken";
    public string AccessTokenKey { get; init; } = "AccessToken";
    public string RefreshTokenKey { get; init; } = "RefreshToken";
    public string ExpiresInKey { get; init; } = "ExpiresIn";
    public IDictionary<string, string> AdditionalTokenKeys { get; init; } = new Dictionary<string, string>();

}