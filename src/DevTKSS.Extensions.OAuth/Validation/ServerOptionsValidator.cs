namespace DevTKSS.Extensions.OAuth.Validation;

public class ServerOptionsValidator : AbstractValidator<ServerOptions>
{
    public ServerOptionsValidator()
    {
        RuleFor(x => x.Protocol)
            .NotEmpty()
            .WithMessage("Protocol must not be empty.");

        RuleFor(x => x.RootUri)
            .NotEmpty()
            .WithMessage("RootUri must not be empty.");

        RuleFor(x => x.Port)
            .InclusiveBetween((ushort)0, (ushort)65535)
            .WithMessage("Port must be between 0 and 65535.");

        RuleFor(x => x.CallbackUri)
            .NotEmpty()
            .Must(uri => uri.StartsWith('/'))
            .WithMessage("CallbackUri must start with '/'.");

        When(x => x.UriFormat == UriFormatMode.Custom, () =>
        {
            RuleFor(x => x.CustomUri)
                .NotEmpty()
                .WithMessage("CustomUri must be set when UriFormat is Custom.");
        });

        When(x => x.UriFormat != UriFormatMode.Custom, () =>
        {
            RuleFor(x => x.CustomUri)
                .Null()
                .WithMessage("CustomUri must be null unless UriFormat is Custom.");
        });
    }
}