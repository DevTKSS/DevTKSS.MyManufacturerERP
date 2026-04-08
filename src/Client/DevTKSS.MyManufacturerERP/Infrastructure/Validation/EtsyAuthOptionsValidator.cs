using System;
using FluentValidation;
using DevTKSS.Extensions.OAuth.Validation;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Validation;

// Validator for EtsyOAuthEndpointOptions leveraging FluentValidation and including OAuthClientOptionsValidator
public sealed class EtsyOAuthEndpointOptionsValidator : AbstractValidator<EtsyOAuthEndpointOptions>
{
    public EtsyOAuthEndpointOptionsValidator()
    {
        // Reuse existing OAuthClientOptionsValidator for base properties
        RuleFor(x => (OAuthClientOptions)x)
            .SetValidator(new OAuthClientOptionsValidator());

        // Validate token key formats
        RuleFor(x => x.UserIdTokenKey)
            .NotEmpty().WithMessage("UserIdTokenKey is required.")
            .Matches("^[A-Za-z0-9_.-]+$").WithMessage("UserIdTokenKey may only contain alphanumeric characters, '.', '_', or '-'.")
            .MaximumLength(64).WithMessage("UserIdTokenKey length exceeds 64 characters.");

        RuleFor(x => x.ShopIdTokenKey)
            .NotEmpty().WithMessage("ShopIdTokenKey is required.")
            .Matches("^[A-Za-z0-9_.-]+$").WithMessage("ShopIdTokenKey may only contain alphanumeric characters, '.', '_', or '-'.")
            .MaximumLength(64).WithMessage("ShopIdTokenKey length exceeds 64 characters.");
    }
}
