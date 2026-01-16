namespace DevTKSS.Extensions.OAuth.Requests;

public record RefreshTokenRequest
{
    [JsonPropertyName(OAuthDefaults.Keys.GrantType)]
    public string GrantType { get; set; } = OAuthDefaults.Values.RefreshToken;

    [JsonPropertyName(OAuthDefaults.Keys.ClientId)]
    public required string ClientId { get; set; }

    [JsonPropertyName(OAuthDefaults.Keys.RefreshToken)]
    public required string RefreshToken { get; set; }

    public IDictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            [OAuthDefaults.Keys.GrantType] = GrantType,
            [OAuthDefaults.Keys.ClientId] = ClientId,
            [OAuthDefaults.Keys.RefreshToken] = RefreshToken,
        };
    }
}