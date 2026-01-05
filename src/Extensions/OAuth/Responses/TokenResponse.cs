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

}
