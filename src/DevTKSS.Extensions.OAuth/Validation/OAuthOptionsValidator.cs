using static DevTKSS.Extensions.OAuth.Validation.UriValidationUtility;
namespace DevTKSS.Extensions.OAuth.Validation;
public class OAuthOptionsValidator : AbstractValidator<OAuthOptions>
{
    public OAuthOptionsValidator()
    {
        RuleFor(x => x.Url)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Url must not be empty.")
            .Must(BeAValidUrl).WithMessage("Url must be a valid URL.");

        RuleSet("ClientOptions", () =>
        {

            RuleFor(x => x.ClientOptions)
                 .Cascade(CascadeMode.Stop)
                 .NotNull().WithMessage("ClientOptions must not be null.")
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types. - not problematic since the 'When' ensures its not null if its executed

                 .SetValidator(new OAuthClientOptionsValidator());
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        });

    }
}
