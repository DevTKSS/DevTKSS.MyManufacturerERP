namespace DevTKSS.Extensions.OAuth.Dictionarys;

public static class TokenCacheExtensions
{
    public static bool TryGetAccessToken(this IDictionary<string, string> cache, out string? accessToken)
    {
        if (cache.TryGetValue(OAuthTokenRefreshDefaults.AccessTokenKey, out var token))
        {
            accessToken = token;
            return true;
        }
        accessToken = null;
        return false;
    }
    public static bool TryGetRefreshToken(this IDictionary<string, string> cache, out string? refreshToken)
    {
        if (cache.TryGetValue(OAuthTokenRefreshDefaults.RefreshToken, out var token))
        {
            refreshToken = token;
            return true;
        }
        refreshToken = null;
        return false;
    }
    public static bool TryGetUserId(this IDictionary<string, string> cache, out string? userId)
    {
        if (cache.TryGetValue(OAuthAuthRequestDefaults.ClientIdKey, out var id))
        {
            userId = id;
            return true;
        }
        userId = null;
        return false;
    }
    public static bool TryGetExpirationDate(this IDictionary<string, string> cache, out DateTime expiresIn)
    {
        if (cache.TryGetValue(OAuthTokenRefreshExtendedDefaults.ExpirationDateKey, out var expires))
        {
            if (DateTime.TryParse(expires, out var value))
            {
                expiresIn = value;
                return true;
            }
        }
        expiresIn = default;
        return false;
    }
}
