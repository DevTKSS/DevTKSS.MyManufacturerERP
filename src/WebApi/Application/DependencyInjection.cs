using System.Reflection;
using MapsterMapper;

namespace DevTKSS.MyManufacturerERP.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Register Mapster
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Pipeline behaviors will be registered in the startup project where Mediator.SourceGenerator is referenced
    }
}
