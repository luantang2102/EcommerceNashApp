using EcommerceNashApp.Web.Models.Views;

namespace EcommerceNashApp.Web.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryView>> GetCategoriesTreeAsync(CancellationToken cancellationToken);
    }
}
