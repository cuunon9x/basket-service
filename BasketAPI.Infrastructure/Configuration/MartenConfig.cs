using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace BasketAPI.Infrastructure.Configuration;

public static class MartenConfig
{
    public static IServiceCollection AddMarten(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL") ??
            throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");

        services.AddMarten(options =>
        {
            options.Connection(connectionString);
              // Configure document mappings
            options.Schema.For<Domain.Entities.ShoppingCart>()
                .Identity(x => x.UserId)
                .UseOptimisticConcurrency(false); // Disable optimistic concurrency for simplicity

            // Schema management - create/update schema automatically in development
            if (configuration.GetValue<bool>("Marten:AutoCreateSchemaObjects", true))
            {
                options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            }            // Configure serialization
            options.UseSystemTextJsonForSerialization(
                enumStorage: EnumStorage.AsString,
                casing: Casing.CamelCase);            // Configure database schema
            options.DatabaseSchemaName = configuration.GetValue<string>("Marten:SchemaName") ?? "basket";
        });

        return services;
    }
}
