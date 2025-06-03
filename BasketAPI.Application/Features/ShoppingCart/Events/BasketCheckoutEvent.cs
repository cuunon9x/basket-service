namespace BasketAPI.Application.Features.ShoppingCart.Events;

public record BasketCheckoutEvent
{
    public string UserName { get; init; } = default!;
    public decimal TotalPrice { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string EmailAddress { get; init; } = default!;
    public string ShippingAddress { get; init; } = default!;
    
    // Payment Information
    public string CardNumber { get; init; } = default!;
    public string CardHolderName { get; init; } = default!;
    public string CardExpiration { get; init; } = default!;
    
    public List<BasketCheckoutItemEvent> Items { get; init; } = new();
}
