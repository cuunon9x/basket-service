using BasketAPI.Domain.Entities;

namespace BasketAPI.Domain.Common;

public interface IBasketRepository
{
    Task<ShoppingCart?> GetBasketAsync(string userName);
    Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket);
    Task DeleteBasketAsync(string userName);
}
