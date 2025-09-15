using static DevTKSS.Extensions.OAuth.Validation.UriValidationUtility;
namespace DevTKSS.Extensions.OAuth.Validation;

public class OAuthClientOptionsValidator : AbstractValidator<OAuthClientOptions>
{
    public OAuthClientOptionsValidator()
    {
        RuleFor(x => x.ProviderName)
            .NotEmpty()
            .WithMessage("ProviderName must not be empty.");

        RuleFor(x => x.ClientID)
            .NotEmpty()
            .WithMessage("ClientID must not be empty.");

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .When(x => x.ClientSecret is not null)
            .WithMessage("ClientSecret must not be empty if provided.");

        RuleFor(x => x.Scopes)
            .NotEmpty()
            .WithMessage("Scopes must not be null or empty.");

        RuleFor(x => x.EndpointOptions)
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types. - not problematic since the 'When' ensures its not null if its executed
            .SetValidator(new OAuthEndpointOptionsValidator())
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            .When(x => x.EndpointOptions is not null);

        RuleFor(x => x.CallbackUri)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("CallbackUri must not be empty.")
            .Must(BeAValidUrl).WithMessage("CallbackUri must be a valid absolute URL.");
    }
}
