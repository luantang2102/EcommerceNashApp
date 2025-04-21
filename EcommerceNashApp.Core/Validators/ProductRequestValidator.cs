using EcommerceNashApp.Core.DTOs.Request;
using FluentValidation;
using Microsoft.AspNetCore.Http;

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

            RuleFor(product => product.FormImages)
                .Must(files => files == null || files.All(file => file != null && file.Length > 0))
                .WithMessage("All uploaded images must have content.")
                .Must(files => files == null || files.All(file => file.Length <= 5 * 1024 * 1024)) // Max 5 MB per file
                .WithMessage("Each image must be less than 5 MB.")
                .Must(files => files == null || files.All(file => IsValidImageType(file)))
                .WithMessage("Only JPEG, PNG, and GIF images are allowed.");
        }

        private bool IsValidImageType(IFormFile file)
        {
            if (file == null) return true;
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            return allowedTypes.Contains(file.ContentType);
        }
    }

    public class ExistingProductImageRequestValidator : AbstractValidator<ExistingProductImageRequest>
    {
        public ExistingProductImageRequestValidator()
        {
            RuleFor(image => image.Id)
                .NotEmpty().WithMessage("Existing image ID is required.");

            RuleFor(image => image.IsMain)
                .NotNull().WithMessage("IsMain must be specified.");
        }
    }
}