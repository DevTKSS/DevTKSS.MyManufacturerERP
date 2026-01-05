using System;

namespace DevTKSS.MyManufacturerERP.Infrastructure;

public class EtsyOAuthEndpointOptions : OAuthEndpointOptions
{
    public new const string SectionName = "EtsyOAuthEndpoint";

    // Token key under which the authenticated user's id will be stored in the token cache/credentials
    public string UserIdTokenKey { get; init; } = "UserId";

    // Token key under which the authenticated user's shop id will be stored (retrieved from /users/getMe)
    public string ShopIdTokenKey { get; init; } = "ShopId";
}
