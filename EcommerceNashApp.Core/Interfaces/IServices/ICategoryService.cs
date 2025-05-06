using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface ICategoryService
    {
        Task<PagedList<CategoryResponse>> GetCategoriesAsync(CategoryParams categoryParams);
        Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId);
        Task<List<CategoryResponse>> GetCategoriesByIdsAsync(List<Guid> categoryIds);
        Task<CategoryResponse> CreateCategoryAsync(CategoryRequest categoryRequest);
        Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, CategoryRequest categoryRequest);
        Task<bool> DeleteCategoryAsync(Guid categoryId);
        Task<List<CategoryResponse>> GetCategoriesTreeAsync();
    }
}
