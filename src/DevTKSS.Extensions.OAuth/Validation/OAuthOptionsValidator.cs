namespace DevTKSS.Extensions.OAuth.Validation;

public class OAuthOptionsValidator : AbstractValidator<OAuthOptions>
{
    public OAuthOptionsValidator()
    {
        RuleFor(x => x.AuthorizationEndpoint)
            .NotEmpty().WithMessage("AuthorizationEndpoint darf nicht leer sein.")
            .Must(BeAValidUrl).WithMessage("AuthorizationEndpoint muss eine gültige URL sein.");

        RuleFor(x => x.UserInfoEndpoint)
            .NotEmpty().WithMessage("UserInfoEndpoint darf nicht leer sein.")
            .Must(BeAValidUrl).WithMessage("UserInfoEndpoint muss eine gültige URL sein.");

        RuleFor(x => x.TokenEndpoint)
            .NotEmpty().WithMessage("TokenEndpoint darf nicht leer sein.")
            .Must(BeAValidUrl).WithMessage("TokenEndpoint muss eine gültige URL sein.");

        RuleFor(x => x.ClientID)
            .NotEmpty().WithMessage("ClientID darf nicht leer sein.");

        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("RedirectUri darf nicht leer sein.")
            .Must(BeAValidUrl).WithMessage("RedirectUri muss eine gültige URL sein.");

        RuleFor(x => x.Scopes)
            .NotNull().WithMessage("Scopes darf nicht null sein.")
            .Must(scopes => scopes.Length > 0).WithMessage("Scopes muss mindestens einen Wert enthalten.");
    }

    private bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
