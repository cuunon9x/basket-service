using BasketAPI.Domain.Common;
using System.Text.Json.Serialization;

namespace BasketAPI.Domain.Entities;

public class ShoppingCart : Entity
{
    public string UserId { get; private set; }
    public List<CartItem> Items { get; private set; } = new(); public ShoppingCart(string userId)
    {
        UserId = userId;
    }    // Constructor for JSON deserialization
    [JsonConstructor]
    public ShoppingCart(Guid id, string userId, List<CartItem> items)
    {
        Id = id;
        UserId = userId;
        Items = items ?? new List<CartItem>();
    }

    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(item.Quantity);
        }
        else
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public decimal GetTotalPrice()
    {
        return Items.Sum(item => item.GetTotalPrice());
    }
}
