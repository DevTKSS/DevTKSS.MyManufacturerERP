using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
