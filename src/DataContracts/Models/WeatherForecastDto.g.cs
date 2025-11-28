using System;

namespace DevTKSS.MyManufacturerERP.DataContracts.GeneratedMappings
{
    public partial class WeatherForecastDto
    {
        public double TemeratureF { get; }
        public double TemperatureC { get; }
        public DateOnly Date { get; }
        public string? Summary { get; }
        
        public WeatherForecastDto(double temeratureF, double temperatureC, DateOnly date, string? summary)
        {
            this.TemeratureF = temeratureF;
            this.TemperatureC = temperatureC;
            this.Date = date;
            this.Summary = summary;
        }
    }
}