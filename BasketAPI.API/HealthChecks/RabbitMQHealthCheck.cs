using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace BasketAPI.API.HealthChecks;

public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly string _username;
    private readonly string _password;

    public RabbitMQHealthCheck(IConfiguration configuration)
    {
        _host = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        _username = configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
        _password = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _host,
                UserName = _username,
                Password = _password,
                Port = 5672,
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                Ssl = { Enabled = false }
            };

            var connection = await factory.CreateConnectionAsync(cancellationToken);
            var channel = await connection.CreateChannelAsync();
            
            // Create a temporary exchange to test
            var exchangeName = $"healthcheck_{Guid.NewGuid()}";
            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, autoDelete: true);
            await channel.ExchangeDeleteAsync(exchangeName);

            await channel.CloseAsync();
            await connection.CloseAsync();
            
            return HealthCheckResult.Healthy("RabbitMQ connection is healthy");
        }
        catch (BrokerUnreachableException ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ broker is unreachable", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection is unhealthy", ex);
        }
    }
}
