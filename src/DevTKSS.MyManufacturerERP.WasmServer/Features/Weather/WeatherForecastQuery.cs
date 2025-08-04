using System.Text.Json.Serialization;

namespace DevTKSS.MyManufacturerERP.DataContracts;

/// <summary>
/// A Weather Forecast for a specific date
/// </summary>
/// <param name="Date">Gets the Date of the Forecast.</param>
/// <param name="TemperatureC">Gets the Forecast Temperature in Celsius.</param>
/// <param name="Summary">Get a description of how the weather will feel.</param>
public record WeatherForecastQuery(DateOnly Date, double TemperatureC, string? Summary)
{
    /// <summary>
    /// Gets the Forecast Temperature in Fahrenheit
    /// </summary>
    public double TemperatureF => 32 + (TemperatureC * 9 / 5);
}

public record WeatherForecast
{
    [JsonPropertyName("temperature_f")]
    public double TemeratureF { get; init; }
    [JsonPropertyName("temperature_c")]
    public double TemperatureC { get; init; }
    [JsonPropertyName("date")]
    public DateOnly Date { get; init; }
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
}