using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.DTOs;
using EcommerceNashApp.Web.Models.Views;
using System.Text.Json;
using System.Web;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient; private readonly ILogger _logger;

        public ProductService(IHttpClientFactory httpClientFactory, ILogger<ProductService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
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
                ProductImages = productDto.ProductImages?.Select(MapProductImageDtoToView).ToList() ?? new List<ProductImageView>()
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

        public ProductRatingView MapRatingDtoToView(RatingDto ratingDto)
        {
            return new ProductRatingView
            {
                Id = ratingDto.Id,
                Value = ratingDto.Value,
                Comment = ratingDto.Comment,
                Username = ratingDto.User.UserName ?? "Anonymous",
                CreatedDate = ratingDto.CreatedDate
            };
        }

        public async Task<PagedList<ProductView>> GetProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken)
        {
            var queryString = $"pageNumber={paginationParams.PageNumber}&pageSize={paginationParams.PageSize}";
            return await FetchProductsAsync($"api/Products?{queryString}", paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
        }

        public async Task<PagedList<ProductView>> GetFilteredProductsAsync(
            string? categories = null,
            string? minPrice = null,
            string? maxPrice = null,
            string? orderBy = null,
            string? searchTerm = null,
            string? ratings = null,
            int pageNumber = 1,
            int pageSize = 12,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new Dictionary<string, string>
        {
            { "PageNumber", pageNumber.ToString() },
            { "PageSize", pageSize.ToString() }
        };
            if (!string.IsNullOrEmpty(categories)) queryParams.Add("Categories", categories);
            if (!string.IsNullOrEmpty(minPrice)) queryParams.Add("MinPrice", minPrice);
            if (!string.IsNullOrEmpty(maxPrice)) queryParams.Add("MaxPrice", maxPrice);
            if (!string.IsNullOrEmpty(orderBy)) queryParams.Add("OrderBy", orderBy);
            if (!string.IsNullOrEmpty(searchTerm)) queryParams.Add("SearchTerm", searchTerm);
            if (!string.IsNullOrEmpty(ratings)) queryParams.Add("Ratings", ratings);

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            return await FetchProductsAsync($"api/Products?{queryString}", pageNumber, pageSize, cancellationToken);
        }

        public async Task<ProductView?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching product with ID {ProductId}", id);
            try
            {
                var response = await _httpClient.GetAsync($"api/Products/{id}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiDto<ProductDto>>(cancellationToken);
                if (apiResponse?.Body != null)
                {
                    return MapProductDtoToView(apiResponse.Body);
                }
                _logger.LogWarning("No product found for ID {ProductId}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed for product ID {ProductId}", id);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize product response for ID {ProductId}", id);
            }
            return null;
        }

        public async Task<List<ProductRatingView>> GetProductRatingsAsync(Guid productId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching ratings for product ID {ProductId}", productId);
            try
            {
                var response = await _httpClient.GetAsync($"api/Ratings/product/{productId}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiDto<IEnumerable<RatingDto>>>(cancellationToken);
                if (apiResponse?.Body != null)
                {
                    return apiResponse.Body.Select(MapRatingDtoToView).ToList();
                }
                _logger.LogWarning("No ratings found for product ID {ProductId}", productId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed for ratings of product ID {ProductId}", productId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize ratings response for product ID {ProductId}", productId);
            }
            return new List<ProductRatingView>();
        }

        private async Task<PagedList<ProductView>> FetchProductsAsync(string requestUri, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching products from {RequestUri}", requestUri);
            try
            {
                var response = await _httpClient.GetAsync(requestUri, cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiDto<List<ProductDto>>>(cancellationToken);

                // Try both "Pagination" and "pagination" headers
                var paginationHeader = response.Headers.Contains("Pagination")
                    ? response.Headers.GetValues("Pagination").FirstOrDefault()
                    : response.Headers.Contains("pagination")
                        ? response.Headers.GetValues("pagination").FirstOrDefault()
                        : null;

                _logger.LogInformation("Pagination Header: {Header}", paginationHeader ?? "null");

                if (apiResponse?.Body != null && paginationHeader != null)
                {
                    try
                    {
                        var pagination = JsonSerializer.Deserialize<PaginationHeader>(
                            paginationHeader,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                        );
                        if (pagination != null)
                        {
                            var productViews = apiResponse.Body.Select(MapProductDtoToView).ToList();
                            var result = new PagedList<ProductView>(
                                productViews,
                                pagination.TotalCount,
                                pagination.CurrentPage,
                                pagination.PageSize
                            );
                            return result;
                        }
                        _logger.LogWarning("Deserialized pagination is null for header: {Header}", paginationHeader);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize pagination header: {Header}", paginationHeader);
                    }
                }
                else
                {
                    _logger.LogWarning("No pagination header or empty response body for {RequestUri}", requestUri);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed for {RequestUri}", requestUri);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize product response for {RequestUri}", requestUri);
            }
            _logger.LogWarning("Returning empty product list for {RequestUri}", requestUri);
            return new PagedList<ProductView>([], 0, pageNumber, pageSize);
        }
    }

}