using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Models;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class ProductImageExtensions
    {
        public static ProductImageResponse MapModelToResponse(this ProductImage productImage)
        {
            return new ProductImageResponse
            {
                Id = productImage.Id,
                ImageUrl = productImage.ImageUrl,
                PublicId = productImage.PublicId,
                IsMain = productImage.IsMain,
                CreatedDate = productImage.CreatedDate
            };
        }
    }
}
