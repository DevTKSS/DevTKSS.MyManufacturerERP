namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;

public class UserDetailsResponse
{
    public long? user_id { get; set; }
    public string? primary_email { get; set; }
    public string? first_name { get; set; }
    public string? last_name { get; set; }
    public string? image_url_75x75 { get; set; }
}
