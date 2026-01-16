namespace DevTKSS.Extensions.OAuth.Validation;

public static class ServiceCollectionExtensions
{
    public static OptionsBuilder<TOptions> AddOptionsWithFluentValidation<TOptions>(this IServiceCollection services, string sectionName)
        where TOptions : class
    {
        return services.AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateFluentValidation();
    }
}
