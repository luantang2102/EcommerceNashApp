using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.Views;

namespace EcommerceNashApp.Web.Services
{
    public interface IProductService
    {
        Task<PagedList<ProductView>> GetProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken);
    }
}
