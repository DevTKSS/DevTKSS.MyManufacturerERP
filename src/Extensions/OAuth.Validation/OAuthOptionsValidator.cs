namespace DevTKSS.Extensions.OAuth.Validation;

public sealed class OAuthOptionsValidator : AbstractValidator<OAuthOptions>
{
    public OAuthOptionsValidator()
    {
        RuleFor(x => x.LoginStartUri)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("LoginStartUri must not be empty.")
            .Must(BeAValidUrl).WithMessage("LoginStartUri must be a valid URL.");

        RuleFor(x => x.LoginCallbackUri)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("LoginCallbackUri must not be empty.")
            .Must(BeAValidUrl).WithMessage("LoginCallbackUri must be a valid URL.");

        RuleFor(x => x.Options)
            .NotNull().WithMessage("Options must not be null.");

        When(x => x.Options is not null, () =>
        {
            RuleFor(x => x.Options!)
                .SetValidator(new OAuthClientOptionsValidator());
        });
    }
}
