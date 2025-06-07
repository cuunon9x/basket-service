using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BasketAPI.Infrastructure.Configuration;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ??
            throw new InvalidOperationException("Connection string 'Redis' not found."); var options = ConfigurationOptions.Parse(redisConnectionString);
        options.ConnectRetry = configuration.GetValue<int>("Redis:ConnectRetry", 5);
        options.ConnectTimeout = configuration.GetValue<int>("Redis:ConnectTimeout", 8000);
        options.SyncTimeout = configuration.GetValue<int>("Redis:OperationTimeout", 5000);
        options.AbortOnConnectFail = false;
        options.ReconnectRetryPolicy = new ExponentialRetry(5000);
        options.ClientName = "BasketAPI";
        options.KeepAlive = 60;

        var redisConnection = ConnectionMultiplexer.Connect(options);
        services.AddSingleton<IConnectionMultiplexer>(redisConnection);

        return services;
    }
}
