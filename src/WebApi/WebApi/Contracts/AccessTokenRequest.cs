using System.Text.Json.Serialization;

namespace DevTKSS.MyManufacturerERP.WebApi.Contracts;

public record AccessTokenRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "authorization_code";

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("code_verifier")]
    public string CodeVerifier { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
