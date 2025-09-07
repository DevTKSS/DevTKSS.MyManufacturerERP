using static DevTKSS.Extensions.OAuth.Validation.UriValidationUtility;
namespace DevTKSS.Extensions.OAuth.Validation;
public class OAuthOptionsValidator : AbstractValidator<OAuthOptions>
{
   
    public OAuthOptionsValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("Url must not be empty.");

        RuleFor(x => x.Url)
            .Must(BeAValidUrl)
            .WithMessage("Url must be a valid URL.");

        RuleSet("EndpointOptions", () =>
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types. - not problematic since the 'When' ensures its not null if its executed
            RuleFor(x => x.EndpointOptions)
                 .SetValidator(new OAuthEndpointOptionsValidator())
                 .When(x => x.EndpointOptions is not null);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        });
        
        RuleFor(x => x.CallbackOptions)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .ChildRules(x =>
                x.RuleFor(x => x!.CallbackUri)
                    .NotEmpty()
                    .WithMessage("CallbackUri must not be empty.")
            );

        RuleFor(x => x.ClientID)
            .NotEmpty()
            .WithMessage("ClientID must not be empty.");

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .When(x => x.ClientSecret is not null)
            .WithMessage("ClientSecret must not be empty if provided.");

        RuleFor(x => x.Scopes)
            .NotNull().WithMessage("Scopes must not be null.")
            .Must(scopes => scopes.Length > 0).WithMessage("Scopes must contain at least one value.");
    }
}
