using BasketAPI.Application.Common.Interfaces;
using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using BasketAPI.Application.Features.ShoppingCart.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Application.Features.ShoppingCart.Commands;

public record UpdateBasketCommand(string UserName, List<CartItem> Items) : IRequest<Domain.Entities.ShoppingCart>;

public class UpdateBasketCommandHandler : IRequestHandler<UpdateBasketCommand, Domain.Entities.ShoppingCart>
{
    private readonly IShoppingCartRepository _repository;
    private readonly IDiscountService _discountService;
    private readonly ILogger<UpdateBasketCommandHandler> _logger;

    public UpdateBasketCommandHandler(
        IShoppingCartRepository repository,
        IDiscountService discountService,
        ILogger<UpdateBasketCommandHandler> logger)
    {
        _repository = repository;
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<Domain.Entities.ShoppingCart> Handle(
        UpdateBasketCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating/Updating basket for user {UserName} with {ItemCount} items",
            request.UserName, request.Items.Count);

        // Create new cart
        var cart = new Domain.Entities.ShoppingCart(request.UserName);        // Apply discounts and add items
        foreach (var requestItem in request.Items)
        {
            // Create new CartItem with proper ID generation
            var cartItem = new CartItem(
                requestItem.ProductId,
                requestItem.ProductName,
                requestItem.UnitPrice,
                requestItem.Quantity);

            await cartItem.ApplyDiscount(_discountService, _logger);
            cart.AddItem(cartItem);
        }

        // Save to repository
        var updatedCart = await _repository.UpdateAsync(cart);

        _logger.LogInformation("Successfully updated basket for user {UserName}. Total items: {ItemCount}",
            updatedCart.UserId, updatedCart.Items.Count);

        return updatedCart;
    }
}
