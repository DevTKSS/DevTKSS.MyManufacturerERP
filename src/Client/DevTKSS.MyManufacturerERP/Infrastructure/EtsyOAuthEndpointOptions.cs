namespace DevTKSS.MyManufacturerERP.Infrastructure;

public class EtsyOAuthEndpointOptions : OAuthClientOptions
{
    public string UserIdTokenKey { get; init; } = "UserId";

    public string ShopIdTokenKey { get; init; } = "ShopId";
}
