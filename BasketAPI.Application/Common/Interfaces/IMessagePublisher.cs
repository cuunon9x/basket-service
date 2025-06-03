using BasketAPI.Application.Features.ShoppingCart.Events;

namespace BasketAPI.Application.Common.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
