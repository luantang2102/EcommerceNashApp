using EcommerceNashApp.Core.Models;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface ICategoryRepository
    {
        IQueryable<Category> GetAllAsync();
        Task<Category?> GetByIdAsync(Guid categoryId);
        Task<List<Category>> GetByIdsAsync(List<Guid> categoryIds);
        Task<Category?> GetWithSubCategoriesAsync(Guid categoryId);
        IQueryable<Category> GetRootCategoriesAsync();
        Task<bool> ParentCategoryExistsAsync(Guid parentCategoryId);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
        Task<bool> HasProduct(Guid categoryId);
    }
}