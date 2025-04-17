using EcommerceNashApp.Core.DTOs.Request;
using FluentValidation;

namespace EcommerceNashApp.Core.Validators
{
    public class ProductRequestValidator : AbstractValidator<ProductRequest>
    {
        public ProductRequestValidator()
        {
            RuleFor(product => product.Name)
                .NotEmpty().WithMessage("Product name is required.");

            RuleFor(product => product.Description)
                .NotEmpty().WithMessage("Product description is required.");

            RuleFor(product => product.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(product => product.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleForEach(product => product.Images)
                .SetValidator(new ExistingProductImageRequestValidator())
                .When(product => product.Images != null && product.Images.Count != 0);
        }
    }

    public class ExistingProductImageRequestValidator : AbstractValidator<ExistingProductImageRequest>
    {
        public ExistingProductImageRequestValidator()
        {
            RuleFor(image => image.ImageUrl)
                .NotEmpty().WithMessage("Image URL is required.");

            RuleFor(image => image.IsMain)
                .NotNull().WithMessage("IsMain must be specified.");
        }
    }
}

