namespace DevTKSS.MyManufacturerERP.Infrastructure.SevDesk;

public class SevDeskClientOptions
{
    public const string SectionName = "SevdeskApiKeyClient";

    public bool UseNativeHandler { get; set; }
    public string? Url { get; set; }
    public string? ApiKey { get; set; }
}
