using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Infrastructure.Helpers.Params;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(ProductRequest productRequest);
        Task DeleteProductAsync(Guid productId);
        Task<ProductResponse> GetProductByIdAsync(Guid productId);
        Task<PagedList<ProductResponse>> GetProductsAsync(ProductParams productParams);
        Task<ProductResponse> UpdateProductAsync(Guid productId, ProductRequest productRequest);
    }
}
