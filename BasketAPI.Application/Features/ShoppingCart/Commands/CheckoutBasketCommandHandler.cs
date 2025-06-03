using BasketAPI.Application.Common.Interfaces;
using BasketAPI.Application.Features.ShoppingCart.Events;
using BasketAPI.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Application.Features.ShoppingCart.Commands;

public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand, bool>
{
    private readonly IShoppingCartRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<CheckoutBasketCommandHandler> _logger;

    public CheckoutBasketCommandHandler(
        IShoppingCartRepository repository,
        IMessagePublisher messagePublisher,
        ILogger<CheckoutBasketCommandHandler> logger)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        // Get existing basket
        var basket = await _repository.GetByUserIdAsync(request.UserName);
        if (basket == null)
        {
            _logger.LogWarning("Basket not found for user {UserName}", request.UserName);
            return false;
        }

        // Create checkout event
        var basketCheckoutEvent = new BasketCheckoutEvent
        {
            UserName = request.UserName,
            TotalPrice = basket.GetTotalPrice(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailAddress = request.EmailAddress,
            ShippingAddress = request.ShippingAddress,
            CardNumber = request.CardNumber,
            CardHolderName = request.CardHolderName,
            CardExpiration = request.CardExpiration,
            Items = basket.Items.Select(item => new BasketCheckoutItemEvent
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };

        try
        {
            // Publish the checkout event
            await _messagePublisher.PublishAsync(basketCheckoutEvent, cancellationToken);
            _logger.LogInformation("BasketCheckoutEvent published for user {UserName}", request.UserName);

            // Delete the basket after successful checkout
            await _repository.DeleteAsync(request.UserName);
            _logger.LogInformation("Basket deleted for user {UserName}", request.UserName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout for user {UserName}", request.UserName);
            return false;
        }
    }
}
