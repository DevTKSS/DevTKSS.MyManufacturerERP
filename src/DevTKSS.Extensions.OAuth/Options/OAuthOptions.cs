namespace DevTKSS.Extensions.OAuth.Options;

public class OAuthOptions : EndpointOptions
{
    public string? AuthorizationEndpoint { get; init; }
    public string? UserInfoEndpoint { get; init; }
    public string? TokenEndpoint { get; init; }
    /// <summary>
    /// The "keystring" aka "ClientID" of your Etsy application, which is used to identify your app when making API requests.
    /// </summary>
    /// <remarks>
    /// Get yours by registring your App on <see href="https://www.etsy.com/developers/your-apps"/></remarks>
    public string? ClientID { get; init; }
    public string? RedirectUri { get; init; }
    public string[] Scopes { get; init; } = [];
    public ValidationResult ValidateOptions()
    {
        var validator = new OAuthOptionsValidator();
        var result = validator.Validate(this);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
        return result;
    }
}
