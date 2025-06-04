using FluentValidation;

namespace BasketAPI.Application.Features.ShoppingCart.Commands;

public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{    public CheckoutBasketCommandValidator()
    {
        RuleFor(v => v.UserName)
            .NotEmpty().WithMessage("UserName is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long");

        RuleFor(v => v.TotalPrice)
            .GreaterThan(0).WithMessage("Total price must be greater than 0");

        RuleFor(v => v.EmailAddress)
            .NotEmpty().WithMessage("EmailAddress is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .MaximumLength(50).WithMessage("FirstName must not exceed 50 characters");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .MaximumLength(50).WithMessage("LastName must not exceed 50 characters");

        RuleFor(v => v.ShippingAddress)
            .NotEmpty().WithMessage("ShippingAddress is required")
            .MaximumLength(180).WithMessage("ShippingAddress must not exceed 180 characters");

        RuleFor(v => v.CardNumber)
            .NotEmpty().WithMessage("CardNumber is required")
            .CreditCard().WithMessage("A valid credit card number is required");

        RuleFor(v => v.CardHolderName)
            .NotEmpty().WithMessage("CardHolderName is required")
            .MaximumLength(100).WithMessage("CardHolderName must not exceed 100 characters");

        RuleFor(v => v.CardExpiration)
            .NotEmpty().WithMessage("CardExpiration is required")
            .Matches(@"^(0[1-9]|1[0-2])\/([0-9]{2})$")
            .WithMessage("CardExpiration must be in MM/YY format");
    }
}
