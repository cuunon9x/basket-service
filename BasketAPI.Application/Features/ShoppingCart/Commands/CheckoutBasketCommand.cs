using BasketAPI.Application.Features.ShoppingCart.Events;
using MediatR;

namespace BasketAPI.Application.Features.ShoppingCart.Commands;

public record CheckoutBasketCommand : IRequest<bool>
{
    public string UserName { get; init; } = default!;
    public decimal TotalPrice { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string EmailAddress { get; init; } = default!;
    public string ShippingAddress { get; init; } = default!;
    public string CardNumber { get; init; } = default!;
    public string CardHolderName { get; init; } = default!;
    public string CardExpiration { get; init; } = default!;
}
