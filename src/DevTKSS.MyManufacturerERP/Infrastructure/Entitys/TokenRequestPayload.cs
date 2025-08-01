namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

public class TokenRequestPayload
{
    public string grant_type { get; set; } = "authorization_code";
    public string client_id { get; set; }
    public string redirect_uri { get; set; }
    public string code { get; set; }
    public string code_verifier { get; set; }
}
