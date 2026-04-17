using FluentValidation;
using StockNova.Application.DTOs.Products;

namespace StockNova.Application.Validators.Products;

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue)
            .WithMessage("Unit price must be greater than or equal to 0");

        RuleFor(x => x.UnitsInStock)
            .GreaterThanOrEqualTo((short)0).When(x => x.UnitsInStock.HasValue)
            .WithMessage("Units in stock must be greater than or equal to 0");
    }
}
