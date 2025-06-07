using BasketAPI.Domain.Common;
using System.Text.Json.Serialization;

namespace BasketAPI.Domain.Entities;

public class CartItem : Entity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public CartItem(Guid productId, string productName, decimal unitPrice, int quantity) : base()
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (unitPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(unitPrice));

        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }// Constructor for JSON deserialization
    [JsonConstructor]
    public CartItem(Guid id, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void SetPrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(newPrice));

        UnitPrice = newPrice;
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Quantity += amount;
    }

    public void DecreaseQuantity(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Quantity -= amount;
        if (Quantity < 0)
        {
            Quantity = 0;
        }
    }

    public decimal GetTotalPrice()
    {
        return UnitPrice * Quantity;
    }
}
