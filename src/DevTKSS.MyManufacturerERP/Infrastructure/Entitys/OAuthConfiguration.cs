using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

public class OAuthConfiguration : OAuth2Configuration
{

    //  public string? ClientSecret { get; set; } // Seems like not actually in use, might be in the past for OAuth 1.0 or Etsy API <v3
}
public class OAuth2Configuration : EndpointOptions
{
    [DefaultValue("https://www.etsy.com/oauth/connect")]
    [Url(ErrorMessage = "Invalid authorization endpoint format.")]
    public string AuthorizationEndpoint { get; init; } = "https://www.etsy.com/oauth/connect";

    [DefaultValue("https://openapi.etsy.com/v3/public/oauth/token")]
    [Url(ErrorMessage = "Invalid token endpoint format.")]
    public string TokenEndpoint { get; init; } = "https://openapi.etsy.com/v3/public/oauth/token";
    /// <summary>
    /// The "keystring" aka "ClientID" of your Etsy application, which is used to identify your app when making API requests.
    /// </summary>
    /// <remarks>
    /// Get yours by registring your App on <see href="https://www.etsy.com/developers/your-apps"/></remarks>
    [Required(ErrorMessage = "API Key is required.")]
    public string ApiKey { get; init; }

    [Required(ErrorMessage = "Redirect URI is required.")]
    [Url(ErrorMessage = "Invalid redirect URI format.")]
    public string RedirectUri { get; init; }

    public string[] Scopes { get; init; } = ["email_r","shop_r"];

    public string? AccessTokenKey { get; init; }
    public string? RefreshTokenKey { get; init; }

}
