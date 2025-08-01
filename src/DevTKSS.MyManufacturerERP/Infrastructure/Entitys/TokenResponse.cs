using System.Text.Json.Serialization;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccesToken { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}
