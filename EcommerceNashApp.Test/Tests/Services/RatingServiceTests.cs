using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Infrastructure.Services;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Shared.Paginations.Service;
using Moq;

namespace EcommerceNashApp.Test.Tests.Services
{
    public class RatingServiceTests
    {
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly RatingService _ratingService;
        private readonly Mock<IPaginationService> _paginationServiceMock;

        public RatingServiceTests()
        {
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _paginationServiceMock = new Mock<IPaginationService>();
            _ratingService = new RatingService(_ratingRepositoryMock.Object, _paginationServiceMock.Object);
        }

        [Fact]
        public async Task CreateRatingAsync_WithValidRequest_ReturnsRatingResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingRequest = new RatingRequest { Value = 5, Comment = "Great!", ProductId = Guid.NewGuid() };
            var user = new AppUser
            {
                Id = userId,
                UserName = "TestUser",
                ImageUrl = "test.jpg",
                PublicId = "publicId",
                Email = "test@gmail.com",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                UserProfiles = new List<UserProfile>
                {
                    new UserProfile
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "John",
                        LastName = "Doe",
                        PhoneNumber = "1234567890",
                        Address = "123 Test St",
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        User = new AppUser
                        {
                            Id = userId,
                            UserName = "TestUser",
                            Email = "testemail@gmail.com"
                        }
                    }
                }
            };
            var rating = new Rating { Id = Guid.NewGuid(), Value = ratingRequest.Value, User = user };
            _ratingRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _ratingRepositoryMock.Setup(r => r.GetByUserAndProductAsync(userId, ratingRequest.ProductId)).ReturnsAsync((Rating)null);
            _ratingRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Rating>())).ReturnsAsync(rating);
            _paginationServiceMock.Setup(p => p.EF_ToPagedList(It.IsAny<IQueryable<RatingResponse>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedList<RatingResponse>(new List<RatingResponse> { rating.MapModelToReponse() }, 1, 1, 1));

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
