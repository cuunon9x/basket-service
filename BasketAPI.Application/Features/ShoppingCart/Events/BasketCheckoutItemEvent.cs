namespace BasketAPI.Application.Features.ShoppingCart.Events;

public record BasketCheckoutItemEvent
{
    public string ProductName { get; init; } = default!;
    public Guid ProductId { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
}
