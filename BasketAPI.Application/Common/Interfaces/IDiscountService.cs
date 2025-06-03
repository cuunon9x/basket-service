namespace BasketAPI.Application.Common.Interfaces;

public interface IDiscountService
{
    Task<(decimal Amount, string Description)> GetDiscountAsync(string productName);
}
