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

        RuleFor(x => x.EndpointOptions.AuthorizationEndpoint)
            .NotEmpty()
            .WithMessage("AuthorizationEndpoint must not be empty.");
            
        RuleFor(x=> x.EndpointOptions.AuthorizationEndpoint)
            .Must(BeAValidUrl)
            .WithMessage("AuthorizationEndpoint must be a valid URL.");

        // UserInfoEndpoint: allow empty, but if not empty, must be a valid relative URL
        RuleFor(x => x.EndpointOptions.UserInfoEndpoint)
            .Must(BeAValidRelativeUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.EndpointOptions.UserInfoEndpoint))
            .WithMessage("UserInfoEndpoint must be a valid relative URL if provided.");

        RuleFor(x => x.EndpointOptions.TokenEndpoint)
            .NotEmpty().WithMessage("TokenEndpoint must not be empty.");

        RuleFor(x=> x.EndpointOptions.TokenEndpoint)
            .Must(BeAValidRelativeUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.EndpointOptions.TokenEndpoint))
            .WithMessage("TokenEndpoint must be a valid URL.");

        RuleFor(x => x.EndpointOptions.RedirectUri)
            .NotEmpty()
            .WithMessage("RedirectUri must not be empty.");

        RuleFor(x => x.EndpointOptions.RedirectUri)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.EndpointOptions.RedirectUri))
            .WithMessage("RedirectUri must be a valid URL.");

        RuleFor(x => x.ClientID)
            .NotEmpty()
            .WithMessage("ClientID must not be empty.");

        RuleFor(x => x.Scopes)
            .NotNull().WithMessage("Scopes must not be null.")
            .Must(scopes => scopes.Length > 0).WithMessage("Scopes must contain at least one value.");
    }

    private static bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var _);
    }

    private static bool BeAValidRelativeUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Relative, out var _);
    }
}
