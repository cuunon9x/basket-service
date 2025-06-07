using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Grpc.Core;
using Grpc.Net.Client;
using BasketAPI.Infrastructure.Services.Grpc;

namespace BasketAPI.API.Configuration;

public static class GrpcConfig
{
    public static IServiceCollection AddGrpcConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add the gRPC exception interceptor
        services.AddTransient<GrpcExceptionInterceptor>();

        // Configure gRPC client with retry policy
        services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcSettings:DiscountUrl"] ??
                throw new InvalidOperationException("GrpcSettings:DiscountUrl is not configured"));
        })
        .ConfigureChannel(options =>
        {
            options.Credentials = ChannelCredentials.Insecure; // For development only
            options.MaxReceiveMessageSize = 1024 * 1024 * 10; // 10MB
            options.MaxSendMessageSize = 1024 * 1024 * 10;    // 10MB
        })
        .AddInterceptor<GrpcExceptionInterceptor>()
        .EnableCallContextPropagation();

        return services;
    }
}
