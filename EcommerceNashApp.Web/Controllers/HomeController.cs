using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models;
using EcommerceNashApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EcommerceNashApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;

    public HomeController(ILogger<HomeController> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsAsync(
            new PaginationParams
            {
                PageNumber = 1,
                PageSize = 8
            },
            cancellationToken
        );

        return View(products);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
