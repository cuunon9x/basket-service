using BasketAPI.Application.Features.ShoppingCart.Commands;
using BasketAPI.Application.Features.ShoppingCart.Events;
using BasketAPI.Domain.Entities;
using Mapster;

namespace BasketAPI.Application.Common.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Shopping Cart Item mappings
        config.NewConfig<CartItem, BasketCheckoutItemEvent>()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ProductName, src => src.ProductName)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.Quantity, src => src.Quantity);        // Shopping Cart mappings
        config.NewConfig<UpdateBasketCommand, ShoppingCart>()
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Items, src => src.Items);

        // Checkout mappings
        config.NewConfig<CheckoutBasketCommand, BasketCheckoutEvent>()
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.EmailAddress, src => src.EmailAddress)
            .Map(dest => dest.ShippingAddress, src => src.ShippingAddress)
            .Map(dest => dest.CardNumber, src => src.CardNumber)
            .Map(dest => dest.CardHolderName, src => src.CardHolderName)
            .Map(dest => dest.CardExpiration, src => src.CardExpiration)
            .Map(dest => dest.TotalPrice, src => src.TotalPrice);
    }
}
