namespace DevTKSS.Extensions.OAuth.Requests;

public record AccessTokenRequest
{
    [JsonPropertyName(OAuthTokenRefreshDefaults.GrantTypeKey)]
    public string GrantType { get; set; } = OAuthTokenRefreshDefaults.RefreshToken;

    [JsonPropertyName(OAuthAuthRequestDefaults.ClientIdKey)]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName(OAuthAuthRequestDefaults.RedirectUriKey)]
    public string RedirectUri { get; set; } = string.Empty;

    [JsonPropertyName(OAuthAuthRequestDefaults.CodeKey)]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName(OAuthPkceDefaults.CodeVerifierKey)]
    public string CodeVerifier { get; set; } = string.Empty;
}
