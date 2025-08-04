using System.ComponentModel.DataAnnotations;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

public class OAuthConfiguration : OAuth2Configuration
{

    //  public string? ClientSecret { get; set; } // Seems like not actually in use, might be in the past for OAuth 1.0 or Etsy API <v3
}
public class OAuth2Configuration : EndpointOptions, IValidatableObject
{

    [Url(ErrorMessage = "AuthorizationEndpoint must be a valid URL.")]
    public string AuthorizationEndpoint { get; init; }
    [Url(ErrorMessage = "TokenEndpoint must be a valid URL.")]
    public string TokenEndpoint { get; init; }
    /// <summary>
    /// The "keystring" aka "ClientID" of your Etsy application, which is used to identify your app when making API requests.
    /// </summary>
    /// <remarks>
    /// Get yours by registring your App on <see href="https://www.etsy.com/developers/your-apps"/></remarks>
    [Required(ErrorMessage = "ClientID is required.")]
    public string ClientID { get; init; }

    public string RedirectUri { get; init; }

    public string[] Scopes { get; init; } = ["email_r", "shop_r"];
    public string? AccessTokenKey { get; init; }
    public string? RefreshTokenKey { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ClientID))
        {
            yield return new ValidationResult("ClientID is required.", new[] { nameof(ClientID) });
        }
        if (Scopes == null || Scopes.Length == 0)
        {
            yield return new ValidationResult("At least one scope is required.", new[] { nameof(Scopes) });
        }
        if (string.IsNullOrWhiteSpace(AuthorizationEndpoint))
        {
            yield return new ValidationResult("AuthorizationEndpoint is required.", new[] { nameof(AuthorizationEndpoint) });
        }
        if (string.IsNullOrWhiteSpace(TokenEndpoint))
        {
            yield return new ValidationResult("TokenEndpoint is required.", new[] { nameof(TokenEndpoint) });
        }
        if (!Uri.IsWellFormedUriString(AuthorizationEndpoint, UriKind.Absolute))
        {
            yield return new ValidationResult("AuthorizationEndpoint must be a valid URL.", new[] { nameof(AuthorizationEndpoint) });
        }
        if (!Uri.IsWellFormedUriString(TokenEndpoint, UriKind.Absolute))
        {
            yield return new ValidationResult("TokenEndpoint must be a valid URL.", new[] { nameof(TokenEndpoint) });
        }
        if (!Uri.IsWellFormedUriString(RedirectUri, UriKind.Absolute))
        {
            yield return new ValidationResult("RedirectUri must be a valid URL.", new[] { nameof(RedirectUri) });
        }

    }
}