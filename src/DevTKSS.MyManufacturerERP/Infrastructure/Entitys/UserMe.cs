using System.Text.Json.Serialization;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

public class UserMe
{
    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }
    [JsonPropertyName("shop_id")]
    public long? ShopId { get; set; }
}
