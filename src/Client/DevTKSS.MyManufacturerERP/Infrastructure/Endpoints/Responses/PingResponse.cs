namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

public record PingResponse
{
    [JsonPropertyName("application_id")]
    public int ApplicationId { get; init; }
}