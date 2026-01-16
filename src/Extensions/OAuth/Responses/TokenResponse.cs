namespace DevTKSS.Extensions.OAuth.Responses;

public record TokenResponse
{
    [JsonPropertyName(OAuthDefaults.Keys.AccessToken)]
    public string? AccessToken { get; set; }
    [JsonPropertyName(OAuthDefaults.Keys.TokenType)]
    public string? TokenType { get; set; }
    [JsonPropertyName(OAuthDefaults.Keys.ExpiresIn)]
    public int ExpiresIn { get; set; } // in seconds
    [JsonPropertyName(OAuthDefaults.Keys.RefreshToken)]
    public string? RefreshToken { get; set; }

    public IDictionary<string,string>? ToDictionary(bool IncludeTokenType = false)
    {
        if (this is not { AccessToken: not null, RefreshToken: not null, TokenType: not null , ExpiresIn: > 0 })
        {
            return default;
        }
        var dict = new Dictionary<string, string>
        {
            [OAuthDefaults.Keys.AccessToken] = AccessToken,
            [OAuthDefaults.Keys.ExpiresIn] = DateTime.Now.AddSeconds(ExpiresIn).ToString("g"),
            [OAuthDefaults.Keys.RefreshToken] = RefreshToken,
        };

        if (IncludeTokenType)
        {
            dict[OAuthDefaults.Keys.TokenType] = TokenType;
        }

        return dict;

    }
}
