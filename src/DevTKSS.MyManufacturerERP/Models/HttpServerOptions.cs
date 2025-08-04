using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

internal record HttpServerOptions : IValidatableObject
{
    [Url(ErrorMessage = "Invalid URL format for Domain.")]
    [DefaultValue("localhost")]
    public string Domain { get; init; } = "localhost";

    [DefaultValue(5001)]
    public int Port { get; init; } = 5001;

    [DefaultValue(true)]
    public bool UseHttps { get; init; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Domain))
        {
            yield return new ValidationResult(
                "Domain must not be empty.",
                new[] { nameof(Domain) });
        }

        if (Port < 1 || Port > 65535)
        {
            yield return new ValidationResult(
                "Port must be between 1 and 65535.",
                new[] { nameof(Port) });
        }
    }
}
