using EcommerceNashApp.Core.DTOs.Request;
using FluentValidation;

namespace EcommerceNashApp.Core.Validators
{
    public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
    {
        public CategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("IsActive must be specified.");

            RuleFor(x => x.ParentCategoryId)
                .Must(id => id == null || Guid.TryParse(id.ToString(), out _))
                .WithMessage("ParentCategoryId must be null or a valid GUID.");
        }
    }
}

