using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Core.Interfaces.IServices
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
