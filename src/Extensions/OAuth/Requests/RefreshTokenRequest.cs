namespace DevTKSS.Extensions.OAuth.Requests;

public record RefreshTokenRequest
{
    [JsonPropertyName(OAuthDefaults.Keys.GrantType)]
    public string GrantType { get; set; } = OAuthDefaults.Values.RefreshToken;

    [JsonPropertyName(OAuthDefaults.Keys.ClientId)]
    public required string ClientId { get; set; }

    [JsonPropertyName(OAuthDefaults.Keys.RefreshToken)]
    public required string RefreshToken { get; set; }
}