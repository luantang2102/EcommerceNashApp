using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using Microsoft.AspNetCore.Authorization;
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
            Response.AddPaginationHeader(products.Metadata);
            return Ok(new ApiResponse<IEnumerable<ProductResponse>>(200, "Products retrieved successfully", products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(new ApiResponse<ProductResponse>(200, "Product retrieved successfully", product));
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategoryId(Guid categoryId, [FromQuery] ProductParams productParams)
        {
            var products = await _productService.GetProductsByCategoryIdAsync(categoryId, productParams);
            Response.AddPaginationHeader(products.Metadata);
            return Ok(new ApiResponse<IEnumerable<ProductResponse>>(200, "Products retrieved successfully", products));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductRequest productRequest)
        {
            var createdProduct = await _productService.CreateProductAsync(productRequest);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, new ApiResponse<ProductResponse>(201, "Product created successfully", createdProduct));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] ProductRequest productRequest)
        {
            var updatedProduct = await _productService.UpdateProductAsync(id, productRequest);
            return Ok(new ApiResponse<ProductResponse>(200, "Product updated successfully", updatedProduct));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new ApiResponse<string>(200, "Product deleted successfully", "Deleted"));
        }
    }
}
