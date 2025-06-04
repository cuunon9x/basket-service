using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BasketAPI.Infrastructure.Configuration;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {        var redisConnectionString = configuration.GetConnectionString("Redis") ??
            throw new InvalidOperationException("Connection string 'Redis' not found.");

        var options = ConfigurationOptions.Parse(redisConnectionString);
        options.ConnectRetry = 3;
        options.ConnectTimeout = 5000; // 5 seconds
        options.SyncTimeout = 5000;
        options.AbortOnConnectFail = false;
        options.ReconnectRetryPolicy = new ExponentialRetry(5000); // Start at 5s, then backoff

        var redisConnection = ConnectionMultiplexer.Connect(options);
        services.AddSingleton<IConnectionMultiplexer>(redisConnection);

        return services;
    }
}
