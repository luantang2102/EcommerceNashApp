using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.Views;

public interface IProductService
{
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
    Task<ProductView> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ProductRatingView>> GetProductRatingsAsync(Guid productId, CancellationToken cancellationToken);
    ProductView MapProductResponseToView(ProductResponse productResponse);
    ProductImageView MapProductImageResponseToView(ProductImageResponse productImageResponse);
    Task<PagedList<ProductView>> GetFeaturedProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken);
}