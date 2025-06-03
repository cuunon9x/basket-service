using BasketAPI.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Infrastructure.Services;

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<MessagePublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _logger.LogInformation("Publishing message of type {MessageType}", typeof(T).Name);
            await _publishEndpoint.Publish(message, cancellationToken);
            _logger.LogInformation("Successfully published message of type {MessageType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message of type {MessageType}", typeof(T).Name);
            throw;
        }
    }
}
