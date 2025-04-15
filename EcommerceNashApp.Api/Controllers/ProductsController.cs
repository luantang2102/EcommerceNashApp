using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(new ApiResponse<ProductResponse>(200, "Product retrieved successfully", product));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductRequest productRequest)
        {
            var createdProduct = await _productService.CreateProductAsync(productRequest);
            return Ok(new ApiResponse<ProductResponse>(201, "Product created successfully", createdProduct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] ProductRequest productRequest)
        {
            var updatedProduct = await _productService.UpdateProductAsync(id, productRequest);
            return Ok(new ApiResponse<ProductResponse>(200, "Product updated successfully", updatedProduct));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new ApiResponse<string>(200, "Product deleted successfully", "Deleted"));
        }
    }
}
