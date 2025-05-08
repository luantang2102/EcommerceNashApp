using EcommerceNashApp.Application.Services;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;
using Moq;

namespace EcommerceNashApp.Test.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly CategoryService _categoryService;
        private readonly Mock<IPaginationService> _paginationServiceMock;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _paginationServiceMock = new Mock<IPaginationService>();
            _categoryService = new CategoryService(_categoryRepositoryMock.Object, _paginationServiceMock.Object);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsPagedList()
        {
            // Arrange
            var categoryParams = new CategoryParams { PageNumber = 1, PageSize = 10 };
            var categories = new List<Category> { new Category { Id = Guid.NewGuid(), Name = "Test Category", Description = "Test Desc" } };
            _categoryRepositoryMock.Setup(r => r.GetAllAsync()).Returns(categories.AsQueryable());
            _paginationServiceMock
                .Setup(p => p.EF_ToPagedList(It.IsAny<IQueryable<CategoryResponse>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedList<CategoryResponse>(categories.Select(c => c.MapModelToResponse()).ToList(), 1, 1, 10));

            // Act
            var result = await _categoryService.GetCategoriesAsync(categoryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Category", result[0].Name);
        }

        [Fact]
        public async Task CreateCategoryAsync_WithValidRequest_ReturnsCategoryResponse()
        {
            // Arrange
            var categoryRequest = new CategoryRequest { Name = "New Category", Description = "Desc", IsActive = true };
            var category = new Category { Id = Guid.NewGuid(), Name = categoryRequest.Name, Description = "Test Desc" };
            _categoryRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(category);

            // Act
            var result = await _categoryService.CreateCategoryAsync(categoryRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryRequest.Name, result.Name);
            _categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Once());
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryHasSubCategories_ThrowsAppException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                SubCategories = new List<Category> { new Category
                                                    {
                                                        Name = "Sub Category Test",
                                                        Description = "Test Desc"
                                                    }},
                Name = "Category Test",
                Description = "Desc"
            };
            _categoryRepositoryMock.Setup(r => r.GetWithSubCategoriesAsync(categoryId)).ReturnsAsync(category);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _categoryService.DeleteCategoryAsync(categoryId));
            Assert.Equal(ErrorCode.CATEGORY_HAS_SUBCATEGORIES, exception.GetErrorCode());
        }
    }
}
