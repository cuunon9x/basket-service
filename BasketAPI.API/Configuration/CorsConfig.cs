using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BasketAPI.API.Configuration;

public static class CorsConfig
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("BasketCorsPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()     // In production, replace with specific origins
                    .AllowAnyMethod()      // In production, specify allowed methods
                    .AllowAnyHeader()      // In production, specify allowed headers
                    .WithExposedHeaders("X-Pagination"); // Expose pagination headers
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors("BasketCorsPolicy");
        return app;
    }
}
