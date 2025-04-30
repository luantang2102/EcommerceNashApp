using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Services;
using Moq;

namespace EcommerceNashApp.Test.Tests.Services
{
    public class RatingServiceTests
    {
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly RatingService _ratingService;

        public RatingServiceTests()
        {
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _ratingService = new RatingService(_ratingRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateRatingAsync_WithValidRequest_ReturnsRatingResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingRequest = new RatingRequest { Value = 5, Comment = "Great!", ProductId = Guid.NewGuid() };
            var user = new AppUser { Id = userId };
            var rating = new Rating { Id = Guid.NewGuid(), Value = ratingRequest.Value };
            _ratingRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _ratingRepositoryMock.Setup(r => r.GetByUserAndProductAsync(userId, ratingRequest.ProductId)).ReturnsAsync((Rating)null);
            _ratingRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Rating>())).ReturnsAsync(rating);

            // Act
            var result = await _ratingService.CreateRatingAsync(userId, ratingRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ratingRequest.Value, result.Value);
            _ratingRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Rating>()), Times.Once());
        }

        [Fact]
        public async Task CreateRatingAsync_WhenRatingExists_ThrowsAppException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingRequest = new RatingRequest { Value = 5, ProductId = Guid.NewGuid() };
            var user = new AppUser { Id = userId };
            var existingRating = new Rating { Id = Guid.NewGuid() };
            _ratingRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _ratingRepositoryMock.Setup(r => r.GetByUserAndProductAsync(userId, ratingRequest.ProductId)).ReturnsAsync(existingRating);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _ratingService.CreateRatingAsync(userId, ratingRequest));
            Assert.Equal(ErrorCode.RATING_ALREADY_EXISTS, exception.GetErrorCode());
        }
    }
}
