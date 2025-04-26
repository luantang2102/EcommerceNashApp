using EcommerceNashApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Web.Controllers.Components
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoryMenuViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryService.GetCategoriesTreeAsync(cancellationToken);
            return View(categories);
        }
    }
}
