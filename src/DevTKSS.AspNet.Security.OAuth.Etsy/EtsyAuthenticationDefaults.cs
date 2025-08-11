namespace DevTKSS.AspNet.Security.OAuth.Etsy;
public class EtsyAuthenticationDefaults
{
    /// <summary>
    /// Default value for <see cref="AuthenticationScheme.Name"/>.
    /// </summary>
    public const string AuthenticationScheme = "Etsy";
    /// <summary>
    /// Default value for <see cref="AuthenticationHeaderValue.Scheme"/>.
    public const string AuthorizationHeaderScheme = "Bearer";
    /// <summary>
    /// Default value for <see cref="AuthenticationScheme.DisplayName"/>.
    /// </summary>
    public static readonly string DisplayName = "Etsy";
    /// <summary>
    /// Default value for <see cref="AuthenticationSchemeOptions.ClaimsIssuer"/>.
    /// </summary>
    public static readonly string Issuer = "Etsy";
    /// <summary>
    /// Default value for <see cref="RemoteAuthenticationOptions.CallbackPath"/>.
    /// </summary>
    public static readonly string CallbackPath = "/auth-callback-etsy";
    /// <summary>
    /// Default value for <see cref="OAuthOptions.AuthorizationEndpoint"/>.
    /// </summary>
    public static readonly string AuthorizationEndpoint = "https://www.etsy.com/oauth/connect";
    /// <summary>
    /// Default value for <see cref="OAuthOptions.TokenEndpoint"/>.
    /// </summary>
    public static readonly string TokenEndpoint = "https://api.etsy.com/v3/public/oauth/token";
    /// <summary>
    /// Default value for <see cref="OAuthOptions.UserInformationEndpoint"/>.
    /// </summary>
    public static readonly string UserInformationEndpoint = "https://openapi.etsy.com/v3/application/users/me";
    /// <summary>
    /// Default value for <see cref="OAuthConstants.CodeChallengeMethodS256"/>.
    /// </summary>
    public static readonly string CodeChallengeMethod = "S256";
}
