using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;
[Headers($"Content-Type: {MediaTypeNames.Application.Json}")]

public interface IEtsyPingEndpoint
{
    [Get("/v3/application/openapi-ping")]
    Task<PingResponse> Ping(
        [Header("x-api-key")] string apiKey);

}
