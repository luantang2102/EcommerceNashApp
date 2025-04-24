using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.DTOs;
using EcommerceNashApp.Web.Models.Views;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class ProductService: IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;

        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public ProductView MapProductDtoToView(ProductDto productDto)
        {
            return new ProductView
            {
                Id = productDto.Id,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                AverageRating = productDto.AverageRating,
                ProductImages = productDto.ProductImages.Select(image => MapProductImageDtoToView(image)).ToList()
            };
        }

        public ProductImageView MapProductImageDtoToView(ProductImageDto productImageDto)
        {
            return new ProductImageView
            {
                Id = productImageDto.Id,
                ImageUrl = productImageDto.ImageUrl,
                PublicId = productImageDto.PublicId,
                IsMain = productImageDto.IsMain
            };
        }

        public async Task<PagedList<ProductView>> GetProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5001/api/Products?pageNumber={paginationParams.PageNumber}&pageSize={paginationParams.PageSize}");

            // Ensure _httpClient.BaseAddress is not null before using it
            var baseAddress = _httpClient.BaseAddress?.ToString() ?? "UnknownBaseAddress";
            _logger.LogInformation("Fetching products from {RequestUri}", baseAddress + request);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            // Fixing the generic type usage and syntax issues
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiDto<List<ProductDto>>>(cancellationToken);

            if (apiResponse?.Body != null)
            {
                var productViews = apiResponse.Body.Select(product => MapProductDtoToView(product)).ToList();

                return new PagedList<ProductView>(
                    productViews,
                    productViews.Count,
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );
            }

            return new PagedList<ProductView>(new List<ProductView>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
        }
    }
}
