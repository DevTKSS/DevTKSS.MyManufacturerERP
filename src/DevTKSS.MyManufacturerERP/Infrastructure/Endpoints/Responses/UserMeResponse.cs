using System.Text.Json.Serialization;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

public class UserMeResponse
{
    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }
    [JsonPropertyName("shop_id")]
    public long? ShopId { get; set; }
}
