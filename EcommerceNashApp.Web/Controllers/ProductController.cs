using Microsoft.AspNetCore.Mvc;
using EcommerceNashApp.Web.Models;
using EcommerceNashApp.Web.Models.Views;
using EcommerceNashApp.Web.Services;
using System.Diagnostics;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Web.Controllers
{
    [Route("Products")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(
            string? categories = null,
            string? minPrice = null,
            string? maxPrice = null,
            string? sortBy = "newest",
            string? searchTerm = null,
            int page = 1,
            int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Loading products with filters: categories={Categories}, minPrice={MinPrice}, maxPrice={MaxPrice}, sortBy={SortBy}, searchTerm={SearchTerm}, page={Page}, pageSize={PageSize}",
                    categories, minPrice, maxPrice, sortBy, searchTerm, page, pageSize);

                if (page < 1)
                {
                    _logger.LogWarning("Invalid page number {Page}, defaulting to 1", page);
                    page = 1;
                }

                if (!string.IsNullOrEmpty(minPrice) && (!double.TryParse(minPrice, out var min) || min < 0 || min > 5000000))
                {
                    _logger.LogWarning("Invalid minPrice {MinPrice}, setting to null", minPrice);
                    minPrice = null;
                }
                if (!string.IsNullOrEmpty(maxPrice) && (!double.TryParse(maxPrice, out var max) || max < 0 || max > 5000000))
                {
                    _logger.LogWarning("Invalid maxPrice {MaxPrice}, setting to null", maxPrice);
                    maxPrice = null;
                }
                if (minPrice != null && maxPrice != null && double.Parse(minPrice) > double.Parse(maxPrice))
                {
                    _logger.LogWarning("minPrice {MinPrice} exceeds maxPrice {MaxPrice}", minPrice, maxPrice);
                    ModelState.AddModelError("minPrice", "Minimum price cannot exceed maximum price.");
                }

                var orderBy = sortBy switch
                {
                    "price-asc" => "priceAsc",
                    "price-desc" => "priceDesc",
                    "popular" => "popular",
                    "newest" => "dateDesc",
                    _ => "dateDesc"
                };

                var allCategories = await _categoryService.GetCategoriesTreeAsync(cancellationToken) ?? new List<CategoryView>();
                var productResult = await _productService.GetFilteredProductsAsync(
                    categories,
                    minPrice,
                    maxPrice,
                    orderBy,
                    searchTerm,
                    null,
                    page,
                    pageSize,
                    cancellationToken) ?? new PagedList<ProductView>([], 0, page, pageSize);

                var selectedCategoryIds = string.IsNullOrEmpty(categories)
                    ? []
                    : categories.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                        .Where(guid => guid != Guid.Empty)
                        .ToList();

                var model = new ProductsPageView
                {
                    Products = productResult.ToList(),
                    Categories = allCategories,
                    CurrentPage = productResult.Metadata.CurrentPage,
                    TotalPages = productResult.Metadata.TotalPages,
                    TotalItems = productResult.Metadata.TotalCount,
                    SelectedCategoryIds = selectedCategoryIds,
                    MinPrice = double.TryParse(minPrice, out var parsedMin) ? parsedMin : null,
                    MaxPrice = double.TryParse(maxPrice, out var parsedMax) ? parsedMax : null,
                    SelectedMinPrice = minPrice,
                    SelectedMaxPrice = maxPrice,
                    SortBy = sortBy,
                    SearchTerm = searchTerm
                };

                if (!ModelState.IsValid)
                {
                    _logger.LogInformation("Returning view with validation errors");
                    return View(model);
                }

                _logger.LogInformation("Rendering products page with {ProductCount} products, page {CurrentPage}/{TotalPages}", model.Products.Count, model.CurrentPage, model.TotalPages);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products page: {Message}", ex.Message);
                return View("Error", new ErrorViewModel
                {
                    Message = "Unable to load products. Please try again later.",
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }
    }
}