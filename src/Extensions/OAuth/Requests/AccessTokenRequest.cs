namespace DevTKSS.Extensions.OAuth.Requests;

public record AccessTokenRequest
{
    [JsonPropertyName(OAuthDefaults.Keys.GrantType)]
    public string GrantType { get; set; } = OAuthDefaults.Values.RefreshToken;

    [JsonPropertyName(OAuthDefaults.Keys.ClientId)]
    public required string ClientId { get; set; }

    [JsonPropertyName(OAuthDefaults.Keys.RedirectUri)]
    public required string RedirectUri { get; set; }

    [JsonPropertyName(OAuthDefaults.Keys.Code)]
    public required string Code { get; set; }

    [JsonPropertyName(OAuthDefaults.Keys.Pkce.CodeVerifier)]
    public required string CodeVerifier { get; set; }
}
