using static DevTKSS.Extensions.OAuth.Validation.UriValidationUtility;
namespace DevTKSS.Extensions.OAuth.Validation;

public class ServerOptionsValidator : AbstractValidator<AuthCallbackOptions>
{
    public ServerOptionsValidator()
    {
        //RuleFor(x => x.Protocol)
        //    .NotEmpty()
        //    .Must(p => p is "http" or "https")
        //    .WithMessage("Protocol must be 'http' or 'https'.");

        //RuleFor(x => x.RootUri)
        //    .NotEmpty()
        //    .WithMessage("RootUri must not be empty.");

        //RuleFor(x => x.Port)
        //    .InclusiveBetween((ushort)0, (ushort)65535)
        //    .WithMessage("Port must be between 0 and 65535.");

        RuleFor(x => x.CallbackUri)
            .NotEmpty()
            .Must(BeAValidUrl)
            .WithMessage("CallbackUri must be a valid relative Uri.");
    }

}