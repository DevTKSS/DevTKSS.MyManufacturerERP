using DevTKSS.MyManufacturerERP.DataContracts.GeneratedMappings;

namespace DevTKSS.MyManufacturerERP.Server.Api.Weather;

internal static class WeatherForecastEndpoints
{
    private const string Tag = "Weather";
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// Maps the Weather API endpoints to the application
    /// </summary>
    /// <param name="app">The WebApplication instance</param>
    /// <returns>The configured WebApplication</returns>
    internal static WebApplication MapWeatherApi(this WebApplication app)
    {
        var weatherGroup = app.MapGroup("/api/weather")
            .WithTags(Tag)
            .WithDescription("Weather forecasting endpoints");

        weatherGroup.MapGet("/forecast", GetForecast)
            .WithName(nameof(GetForecast))
            .WithSummary("Gets a 5-day weather forecast")
            .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Creates a make believe weather forecast for the next 5 days.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <returns>A fake 5 day forecast</returns>
    /// <example>
    /// Sample response:
    /// [
    ///   {
    ///     "date": "2025-07-16",
    ///     "temperatureC": 25,
    ///     "temperatureF": 77,
    ///     "summary": "Warm"
    ///   }
    /// ]
    /// </example>
    private static async Task<Ok<WeatherForecastDto[]>> GetForecast(ILogger logger)
    {
        logger.Debug("Getting Weather Forecast for Scalar documentation demonstration.");
        await Task.Delay(TimeSpan.FromSeconds(2)); // Simulate some delay for demonstration purposes
        var forecasts = Enumerable.Range(1, 5).Select(index =>
        {
            var forecast = new WeatherForecastDto(Random.Shared.Next(-20, 55),Random.Shared.Next(-20, 55),DateOnly.FromDateTime(DateTime.Now.AddDays(index)),Summaries[Random.Shared.Next(Summaries.Length)]);

            logger.Information("Weather forecast for {Date} is {Summary} at {TemperatureC}°C ({TemperatureF}°F)",
                forecast.Date, forecast.Summary, forecast.TemperatureC, forecast.Summary);

            return forecast;
        })
        .ToArray();

        logger.Information("Generated {ForecastCount} weather forecasts", forecasts.Length);
        return TypedResults.Ok(forecasts);
    }
}
