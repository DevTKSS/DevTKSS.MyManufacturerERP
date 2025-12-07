using DevTKSS.MyManufacturerERP.Infrastructure.EtsyDefaults;

namespace DevTKSS.Extensions.OAuth.Dictionarys;

public static class EtsyAuthSpecializedTokenCacheExtensions
{
    public static bool TryGetEtsyUserId(this IDictionary<string, string> cache, out long userId)
    {
        if (cache.TryGetValue(OAuthTokenRefreshExtendedDefaults.UserIdKey, out var id))
        {
            if (long.TryParse(id, out var value))
            {
                userId = value;
                return true;
            }
        }
        userId = default;
        return false;
    }

    public static bool TryGetEtsyShopId(this IDictionary<string, string> cache, out long shopId)
    {
        if (cache.TryGetValue($"{OAuthTokenRefreshExtendedDefaults.UserIdKey}_{EtsyOAuthMeRequestDefaults.ShopIdKey}", out var id))
        {
            if (long.TryParse(id, out var value))
            {
                shopId = value;
                return true;
            }
        }
        shopId = default;
        return false;
    }
}
