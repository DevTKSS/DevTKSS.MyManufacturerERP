namespace DevTKSS.Extensions.OAuth.Requests;

public record RefreshTokenRequest
{
    [JsonPropertyName(OAuthTokenRefreshDefaults.GrantTypeKey)]
    public string GrantType { get; set; } = OAuthTokenRefreshDefaults.AuthorizationCode;
    [JsonPropertyName(OAuthAuthRequestDefaults.ClientIdKey)]
    public string ClientId { get; set; } = string.Empty;
    [JsonPropertyName(OAuthTokenRefreshDefaults.RefreshToken)]
    public string RefreshToken { get; set; } = string.Empty;
}