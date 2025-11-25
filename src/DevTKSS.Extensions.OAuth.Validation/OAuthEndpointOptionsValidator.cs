
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
            .Cascade(CascadeMode.Stop)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.UserInfoEndpoint))
            .WithMessage("UserInfoEndpoint must be a valid URL if provided.");

        RuleFor(x => x.TokenEndpoint)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("TokenEndpoint must not be empty.")
            .Must(BeAValidUrl).WithMessage("TokenEndpoint must be a valid URL.");

        RuleFor(x=> x.TokenEndpoint)
            .Cascade(CascadeMode.Stop)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.TokenEndpoint))
            .WithMessage("TokenEndpoint must be a valid URL.");

        RuleFor(x=> x.ClientId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("ClientId must not be empty.");

        RuleFor(x=> x.ClientSecret)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("ClientSecret must not be empty.")
            .When(x => x.UsePkce == true);

        RuleFor(x => x.RedirectUri)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("RedirectUri must not be empty.")
            .Must(BeAValidUrl).WithMessage("RedirectUri must be a valid URL.");

        RuleFor(x => x.Scopes)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("At least one scope must be specified in Scopes.");
    }

}
