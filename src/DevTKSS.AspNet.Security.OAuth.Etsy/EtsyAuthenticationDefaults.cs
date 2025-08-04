using System.Text.Encodings.Web;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;
public class EtsyAuthenticationDefaults
{
    /// <summary>
    /// Represents the authentication scheme used for Etsy integrations.
    /// </summary>
    public const string AuthenticationScheme = "Etsy";

    /// <summary>
    /// Represents the display name for the Etsy platform.
    /// </summary>
    public static readonly string DisplayName = "Etsy";
    /// <summary>
    /// The default callback path for Etsy authentication. This is the path where Etsy will redirect users after they have authenticated.
    /// </summary>
    /// <remarks>
    /// Default value is "/callback-etsy". This path should be registered in your application to handle the OAuth callback.
    /// </remarks>
    public static readonly string CallbackPath = "/auth-callback-etsy";

    /// <summary>
    /// The URL of the OAuth authorization endpoint for Etsy.
    /// </summary>
    /// <remarks>
    /// This endpoint is used to initiate the OAuth authorization process for Etsy API integration.
    /// Applications should redirect users to this URL to obtain authorization for accessing their Etsy
    /// account.
    /// </remarks>
    public static readonly string AuthorizationEndpoint = "https://www.etsy.com/oauth/connect";
    /// <summary>
    /// Default value for the token endpoint, which is used to exchange an authorization code for an access token.
    /// </summary>
    public static readonly string TokenEndpoint = "https://api.etsy.com/v3/public/oauth/token";
    /// <summary>
    /// Default value for the user info endpoint, which is used to retrieve user information after authentication.
    /// </summary>
    public static readonly string UserInfoEndpoint = "/v3/application/userinfo";
    /// <summary>
    /// Gets the default scope used for authentication, encoded for safe inclusion in URLs.
    /// </summary>
    /// <remarks>
    /// "email_r" scope is required for the getMe endpoint to retrieve the bearer token afterwards.
    /// </remarks>
    public static readonly string DefaultScope = UrlEncoder.Default.Encode("email_r");
    /// <summary>
    /// Represents the default response type used in authentication flows.
    /// </summary>
    /// <remarks>
    /// The default value is "code", which is typically used in authorization code flows.
    /// This constant can be used to specify or compare the response type in authentication-related
    /// operations.</remarks>
    public const string DefaultResponseType = "code";
    /// <summary>
    /// Represents the default code challenge method used in the OAuth 2.0 PKCE (Proof Key for Code Exchange) flow.
    /// </summary>
    /// <remarks>
    /// The default value is "S256", which specifies the SHA-256 hashing algorithm.
    /// This is the recommended and more secure method for generating code challenges in PKCE.
    /// </remarks>
    public const string DefaultCodeChallengeMethod = "S256";
}