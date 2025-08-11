using System.Reflection;
using System.Security.Claims;
namespace DevTKSS.AspNet.Security.OAuth.Etsy;

public class EtsyAuthenticationOptions : OAuthOptions
{
    public EtsyAuthenticationOptions()
    {
        ClaimsIssuer = EtsyAuthenticationDefaults.Issuer;
        CallbackPath = EtsyAuthenticationDefaults.CallbackPath;

        AuthorizationEndpoint = EtsyAuthenticationDefaults.AuthorizationEndpoint;
        TokenEndpoint = EtsyAuthenticationDefaults.TokenEndpoint;
        UserInformationEndpoint = EtsyAuthenticationDefaults.UserInformationEndpoint;

        // PKCE is required for Etsy OAuth 2.0
        UsePkce = true;

        Scope.Add("email_r");
        Scope.Add("shops_r");

        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
        ClaimActions.MapJsonKey("shop_id", "shop_id");
    }
}
