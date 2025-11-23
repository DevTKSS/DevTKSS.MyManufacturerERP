namespace DevTKSS.Extensions.OAuth.Responses;

public record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; } // in seconds
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

}
