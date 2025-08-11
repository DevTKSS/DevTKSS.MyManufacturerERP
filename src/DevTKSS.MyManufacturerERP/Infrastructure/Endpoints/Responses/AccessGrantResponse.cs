namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

public record AccessGrantResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }
    [JsonPropertyName("_state")]
    public string? State { get; init; }
    [JsonPropertyName("error")]
    public string? Error { get; init; }
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
    [JsonPropertyName("error_uri")]
    public string? ErrorUri { get; init; }
}
