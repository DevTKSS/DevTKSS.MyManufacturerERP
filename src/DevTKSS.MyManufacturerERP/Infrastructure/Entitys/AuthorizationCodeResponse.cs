using System.Text.Json.Serialization;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

public class AuthorizationCodeResponse
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; init; } = "authorization_code";
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }
    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("code_verifier")]
    public string CodeVerifier { get; set; }
}
