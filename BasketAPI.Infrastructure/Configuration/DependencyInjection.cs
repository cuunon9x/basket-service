using BasketAPI.Application.Common.Interfaces;
using BasketAPI.Domain.Common;
using BasketAPI.Infrastructure.Persistence;
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
    {
        services.AddRedis(configuration);
        services.AddScoped<IShoppingCartRepository, RedisShoppingCartRepository>();
        
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
