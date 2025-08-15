using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;

[Headers("Accept: application/json")]
public interface IEtsyPingEndpoint
{
    [Get("/v3/application/openapi-ping")]
    Task<PingResponse> Ping(
        [Header("x-api-key")] string apiKey);

}
