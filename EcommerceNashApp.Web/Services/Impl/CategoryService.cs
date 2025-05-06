using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using EcommerceNashApp.Web.Models.Views;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IHttpClientFactory httpClient, ILogger<CategoryService> logger)
        {
            _httpClient = httpClient.CreateClient("NashApp.Api");
            _logger = logger;
        }

        public CategoryView MapCategoryResponseToView(CategoryResponse categoryResponse)
        {
            return new CategoryView
            {
                Id = categoryResponse.Id,
                Description = categoryResponse.Description,
                Name = categoryResponse.Name,
                SubCategories = categoryResponse.SubCategories.Select(x => MapCategoryResponseToView(x)).ToList()
            };
        }
        public async Task<List<CategoryView>> GetCategoriesTreeAsync(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Categories/tree");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            // Fixing the generic type usage and syntax issues
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryResponse>>>(cancellationToken);

            if (apiResponse?.Body != null)
            {
                var categoryViews = apiResponse.Body.Select(x => MapCategoryResponseToView(x)).ToList();

                return categoryViews;
            }

            return [];
        }
    }
}
