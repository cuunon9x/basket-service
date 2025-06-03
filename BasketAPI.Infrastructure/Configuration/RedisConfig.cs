using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BasketAPI.Infrastructure.Configuration;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ??
            throw new InvalidOperationException("Connection string 'Redis' not found.");

        var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(redisConnection);

        return services;
    }
}
