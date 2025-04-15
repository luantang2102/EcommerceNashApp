using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductParams productParams)
        {
            var products = await _productService.GetProductsAsync(productParams);

            return Ok(new ApiResponse<IEnumerable<ProductResponse>>(200, "Products retrieved successfully", products));
        }
    }
}
