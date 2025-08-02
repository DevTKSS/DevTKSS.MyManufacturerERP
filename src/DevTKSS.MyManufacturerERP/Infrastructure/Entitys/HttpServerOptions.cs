using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevTKSS.MyManufacturerERP.Infrastructure.Entitys;

internal record HttpServerOptions
{
    [Url(ErrorMessage = "Invalid URL format for Domain.")]
    [DefaultValue("localhost")]
    public string Domain { get; init; } = "localhost";
    [DefaultValue(5001)]
    public int Port { get; init; } = 5001;
    [DefaultValue(true)]
    public bool UseHttps { get; init; } = true;
}
