namespace DevTKSS.Extensions.OAuth.Options;
/// <summary>
/// Options for providing custom or default keys for token response and TokenCache storage.
/// Seperated from the AuthOptions to allow better separation of concerns.
/// </summary>
public record TokenKeyOptions
{
    public const string ConfigurationSection = "TokenKeys";
    public string IdTokenKey { get; init; } = "IdToken";
    public string AccessTokenKey { get; init; } = "AccessToken";
    public string RefreshTokenKey { get; init; } = "RefreshToken";
    public string ExpiresInKey { get; init; } = "ExpiresIn";
    public IDictionary<string, string> AdditionalTokenKeys { get; init; } = new Dictionary<string, string>();

}