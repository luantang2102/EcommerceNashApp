using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Infrastructure.Helpers.Params;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface IProductService
    {
        Task<PagedList<ProductResponse>> GetProductsAsync(ProductParams productParams);
        Task<ProductResponse> GetProductByIdAsync(Guid productId);
        Task<PagedList<ProductResponse>> GetProductsByCategoryIdAsync(Guid categoryId, ProductParams productParams);
        Task<ProductResponse> CreateProductAsync(ProductRequest productRequest);
        Task<ProductResponse> UpdateProductAsync(Guid productId, ProductRequest productRequest);
        Task DeleteProductAsync(Guid productId);

    }
}
