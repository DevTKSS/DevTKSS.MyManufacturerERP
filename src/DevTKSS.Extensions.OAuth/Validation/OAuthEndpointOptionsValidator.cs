using static DevTKSS.Extensions.OAuth.Validation.UriValidationUtility;
namespace DevTKSS.Extensions.OAuth.Validation;

public class OAuthEndpointOptionsValidator : AbstractValidator<OAuthEndpointOptions>
{
    public OAuthEndpointOptionsValidator()
    {

        RuleFor(x => x.AuthorizationEndpoint)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("AuthorizationEndpoint must not be empty.")
            .Must(BeAValidUrl).WithMessage("AuthorizationEndpoint must be a valid URL.");

        // UserInfoEndpoint: allow empty, but if not empty, must be a valid relative URL
        RuleFor(x => x.UserInfoEndpoint)
            .Must(BeAValidRelativeUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.UserInfoEndpoint))
            .WithMessage("UserInfoEndpoint must be a valid relative URL if provided.");

        RuleFor(x => x.TokenEndpoint)
            .NotEmpty()
            .WithMessage("TokenEndpoint must not be empty.");

        RuleFor(x=> x.TokenEndpoint)
            .Must(BeAValidRelativeUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.TokenEndpoint))
            .WithMessage("TokenEndpoint must be a valid URL.");
    }

}
