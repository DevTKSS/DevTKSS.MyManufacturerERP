//using Riok.Mapperly.Abstractions;

namespace DevTKSS.MyManufacturerERP.DataContracts;

[AdaptTo("[name]Dto", MapToConstructor = true), GenerateMapper]
public class WeatherForecast
{
    [JsonPropertyName("temperature_f")]
    public double TemeratureF  => 32 + (TemperatureC * 9 / 5);
    [JsonPropertyName("temperature_c")]
    public double TemperatureC { get; init; }
    [JsonPropertyName("date")]
    public DateOnly Date { get; init; }
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
}
