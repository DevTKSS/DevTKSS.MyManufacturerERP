namespace DevTKSS.MyManufacturerERP.Infrastructure.SevDesk;

public class SevDeskClientOptions : EndpointOptions
{
    public const string SectionName = "SevdeskApiKeyClient";

    public string? ApiKey { get; set; }
}
