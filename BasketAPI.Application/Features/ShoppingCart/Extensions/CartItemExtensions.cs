using BasketAPI.Application.Common.Interfaces;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Application.Features.ShoppingCart.Extensions;

public static class CartItemExtensions
{
    public static async Task ApplyDiscount(this CartItem item, IDiscountService discountService, ILogger<object>? logger = null)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (discountService == null)
            throw new ArgumentNullException(nameof(discountService));

        try
        {
            var (discountAmount, description) = await discountService.GetDiscountAsync(item.ProductName);
            if (discountAmount > 0)
            {
                decimal newPrice = item.UnitPrice - discountAmount;
                
                // Ensure price doesn't go below zero
                if (newPrice < 0)
                    newPrice = 0;
                    
                item.SetPrice(newPrice);
                
                logger?.LogInformation(
                    "Applied discount of {DiscountAmount} ({Description}) to product {ProductName}. New price: {NewPrice}",
                    discountAmount, description, item.ProductName, newPrice);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(
                ex, 
                "Failed to apply discount for product {ProductName}. Using original price {OriginalPrice}", 
                item.ProductName, 
                item.UnitPrice);
        }
    }
}
