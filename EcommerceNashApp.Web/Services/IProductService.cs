using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.DTOs;
using EcommerceNashApp.Web.Models.Views;

namespace EcommerceNashApp.Web.Services
{
    public interface IProductService
    {
        ProductView MapProductDtoToView(ProductDto productDto);
        ProductImageView MapProductImageDtoToView(ProductImageDto productImageDto);
        Task<PagedList<ProductView>> GetProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken);

        Task<PagedList<ProductView>> GetFilteredProductsAsync(
            string? categories = null,
            string? minPrice = null,
            string? maxPrice = null,
            string? orderBy = null,
            string? searchTerm = null,
            string? ratings = null,
            int pageNumber = 1,
            int pageSize = 12,
            CancellationToken cancellationToken = default);

    }
}