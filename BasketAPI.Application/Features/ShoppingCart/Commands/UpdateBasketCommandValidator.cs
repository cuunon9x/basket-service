using FluentValidation;
using BasketAPI.Domain.Entities;

namespace BasketAPI.Application.Features.ShoppingCart.Commands;

public class UpdateBasketCommandValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items cannot be null");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product Id is required");

            item.RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product Name is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit Price must be greater than 0");
        });
    }
}
