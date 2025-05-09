﻿using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    public class CategoriesController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public ICategoryService CategoryService => _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] CategoryParams categoryParams)
        {
            var categories = await CategoryService.GetCategoriesAsync(categoryParams);
            Response.AddPaginationHeader(categories.Metadata);
            return Ok(new ApiResponse<IEnumerable<CategoryResponse>>(200, "Categories retrieved successfully", categories));
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoriesTree()
        {
            var categories = await CategoryService.GetCategoriesTreeAsync();
            return Ok(new ApiResponse<IEnumerable<CategoryResponse>>(200, "Categories tree retrieved successfully", categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await CategoryService.GetCategoryByIdAsync(id);
            return Ok(new ApiResponse<CategoryResponse>(200, "Category retrieved successfully", category));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryRequest categoryRequest)
        {
            var createdCategory = await CategoryService.CreateCategoryAsync(categoryRequest);
            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, new ApiResponse<CategoryResponse>(201, "Category created successfully", createdCategory));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromForm] CategoryRequest categoryRequest)
        {
            var updatedCategory = await CategoryService.UpdateCategoryAsync(id, categoryRequest);
            return Ok(new ApiResponse<CategoryResponse>(200, "Category updated successfully", updatedCategory));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await CategoryService.DeleteCategoryAsync(id);
            return Ok(new ApiResponse<string>(200, "Category deleted successfully", "Deleted"));
        }
    }
}
