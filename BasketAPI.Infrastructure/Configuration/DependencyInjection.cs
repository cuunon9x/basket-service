using BasketAPI.Application.Common.Interfaces;
using BasketAPI.Domain.Common;
using BasketAPI.Infrastructure.Metrics;
using BasketAPI.Infrastructure.Persistence;
using BasketAPI.Infrastructure.Persistence.Decorators;
using Scrutor;
using BasketAPI.Infrastructure.Services;
using BasketAPI.Infrastructure.Services.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasketAPI.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add PostgreSQL with Marten
        services.AddMarten(configuration);

        // Add Redis
        services.AddRedis(configuration);

        // Register metrics
        services.AddSingleton<IMetrics, PrometheusMetrics>();        // Register repositories using decorator pattern
        // 1. Base repository (PostgreSQL with Marten)
        services.AddScoped<MartenShoppingCartRepository>();
        services.AddScoped<IBasketRepository, MartenShoppingCartRepository>();

        // 2. Apply decorators in order (innermost to outermost)
        // Note: The order is important - decorators are applied from inside out
        services.Decorate<IBasketRepository, MetricsShoppingCartRepositoryDecorator>();
        services.Decorate<IBasketRepository, LoggingShoppingCartDecorator>();
        services.Decorate<IBasketRepository, CachingShoppingCartRepositoryDecorator>();

        // Register discount service (stub for this containerized setup)
        services.AddScoped<IDiscountService, StubDiscountService>();

        // Register message publisher
        services.AddScoped<IMessagePublisher, SimpleMessagePublisher>();

        return services;
    }
}
