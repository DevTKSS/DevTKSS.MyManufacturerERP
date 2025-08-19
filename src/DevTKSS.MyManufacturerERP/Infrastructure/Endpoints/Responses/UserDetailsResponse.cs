namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

public class UserDetailsResponse
{
    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("primary_email")]
    public string? PrimaryEmail { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("image_url_75x75")]
    public string? ImageUrl75x75 { get; set; }
}
