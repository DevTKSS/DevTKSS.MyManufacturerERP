namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

public record UserMeResponse
{
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
    [JsonPropertyName("shop_id")]
    public long ShopId { get; set; }
}
