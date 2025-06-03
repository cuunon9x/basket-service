using BasketAPI.Domain.Entities;

namespace BasketAPI.Domain.Common;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(string userId);
    Task<ShoppingCart> UpdateAsync(ShoppingCart cart);
    Task DeleteAsync(string userId);
}
