using BasketAPI.Domain.Common;

namespace BasketAPI.Domain.Entities;

public class ShoppingCart : Entity
{
    public string UserId { get; private set; }
    public List<CartItem> Items { get; private set; } = new();

    public ShoppingCart(string userId)
    {
        UserId = userId;
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
