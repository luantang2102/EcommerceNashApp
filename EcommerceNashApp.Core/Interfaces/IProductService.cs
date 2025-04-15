using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Infrastructure.Helpers.Params;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface IProductService
    {
        Task<PagedList<ProductResponse>> GetProductsAsync(ProductParams productParams);
    }
}
