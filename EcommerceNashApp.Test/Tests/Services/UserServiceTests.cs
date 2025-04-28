using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Services;
using Moq;

namespace EcommerceNashApp.Test.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsPagedList()
        {
            // Arrange
            var userParams = new UserParams { PageNumber = 1, PageSize = 10 };
            var users = new List<AppUser> { new AppUser { Id = Guid.NewGuid(), UserName = "TestUser" } };
            _userRepositoryMock.Setup(r => r.GetUsersInRoleAsync("User")).ReturnsAsync(users);
            _userRepositoryMock.Setup(r => r.GetAllAsync()).Returns(users.AsQueryable());
            _userRepositoryMock.Setup(r => r.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _userService.GetUsersAsync(userParams);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TestUser", result[0].UserName);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ReturnsUserResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "TestUser" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.IsInRoleAsync(user, "User")).ReturnsAsync(true);
            _userRepositoryMock.Setup(r => r.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("TestUser", result.UserName);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserNotFound_ThrowsAppException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((AppUser?)null); 

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _userService.GetUserByIdAsync(userId));
            Assert.Equal(ErrorCode.USER_NOT_FOUND, exception.GetErrorCode());
        }
    }
}
