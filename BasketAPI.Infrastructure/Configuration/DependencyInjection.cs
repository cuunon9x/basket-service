using BasketAPI.Application.Common.Interfaces;
using BasketAPI.Domain.Common;
using BasketAPI.Infrastructure.Metrics;
using BasketAPI.Infrastructure.Persistence;
using BasketAPI.Infrastructure.Persistence.Decorators;
using Scrutor;
using BasketAPI.Infrastructure.Services;
using BasketAPI.Infrastructure.Services.Grpc;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasketAPI.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {        services.AddRedis(configuration);
        
        // Register metrics
        services.AddSingleton<IMetrics, PrometheusMetrics>();

        // Register base repository
        services.AddScoped<RedisShoppingCartRepository>();
        services.AddScoped<IShoppingCartRepository, RedisShoppingCartRepository>();

        // Register decorators in order (innermost to outermost)
        // 1. Metrics - measure performance metrics
        // 2. Logging - log operations
        // 3. Caching - cache results for performance
        services.Decorate<IShoppingCartRepository, MetricsShoppingCartRepositoryDecorator>();
        services.Decorate<IShoppingCartRepository, LoggingShoppingCartDecorator>();
        services.Decorate<IShoppingCartRepository, CachingShoppingCartDecorator>();
        
        // Configure gRPC Client
        services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
        {
            options.Address = new Uri(configuration.GetValue<string>("GrpcSettings:DiscountUrl")!);
        });
        
        services.AddScoped<IDiscountGrpcService, DiscountGrpcService>();
        services.AddScoped<IDiscountService, DiscountService>();

        // Configure MassTransit
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]);
                    h.Password(configuration["RabbitMQ:Password"]);
                });

                // Configure endpoints here if needed
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IMessagePublisher, MessagePublisher>();

        return services;
    }
}
