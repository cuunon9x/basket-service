using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace BasketAPI.Application.Common.Mappings;

public static class MapsterConfiguration
{
    public static IServiceCollection AddApplicationMappings(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(MapsterConfiguration).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
