using System;
using FluentValidation;
using DevTKSS.Extensions.OAuth.Validation;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Validation;

// Validator for EtsyOAuthEndpointOptions leveraging FluentValidation and including OAuthEndpointOptionsValidator
public sealed class EtsyOAuthEndpointOptionsValidator : AbstractValidator<EtsyOAuthEndpointOptions>
{
    public EtsyOAuthEndpointOptionsValidator()
    {
        // Reuse existing OAuthEndpointOptionsValidator for base properties
        RuleFor(x => (OAuthEndpointOptions)x)
            .SetValidator(new OAuthEndpointOptionsValidator());

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
