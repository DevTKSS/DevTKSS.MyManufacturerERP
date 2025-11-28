namespace DevTKSS.Extensions.OAuth.Endpoints;

public record TokenKeyOptions
{
    public const string ConfigurationSection = "TokenKeyOptions";
    public string? IdTokenKey { get; init; }
    public string? AccessTokenKey { get; init; }
    public string? RefreshTokenKey { get; init; }

    public IDictionary<string, string>? AdditionalTokenKeys { get; init; }

}