using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.Views;
using EcommerceNashApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

public class ProductDetailsController : Controller
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductDetailsController> _logger;

    public ProductDetailsController(IProductService productService, ILogger<ProductDetailsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
    {
        // Fetch product details (assume an API endpoint or service method)
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        // Fetch related products (all products for now)
        var relatedProducts = await _productService.GetProductsAsync(
            new PaginationParams { PageNumber = 1, PageSize = 10 },
            cancellationToken
        );

        // Fetch ratings (assume an API endpoint or service method)
        var ratings = await _productService.GetProductRatingsAsync(id, cancellationToken);

        var model = new ProductDetailsView
        {
            Product = product,
            RelatedProducts = relatedProducts.ToList(),
            Ratings = ratings?.ToList() ?? new List<ProductRatingView>(),
            IsLoggedIn = User.Identity.IsAuthenticated // Check if user is logged in
        };

        return View(model);
    }
}