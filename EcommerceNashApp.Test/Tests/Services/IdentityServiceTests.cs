using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices.Auth;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Services.Auth;
using Moq;

namespace EcommerceNashApp.Tests.Services
{
    public class IdentityServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly IdentityService _identityService;

        public IdentityServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _identityService = new IdentityService(_userRepositoryMock.Object, _jwtServiceMock.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokenResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "Password123!" };
            var user = new AppUser { Id = Guid.NewGuid(), Email = loginRequest.Email, UserName = "TestUser" };
            var roles = new List<string> { "User" };
            _userRepositoryMock.Setup(r => r.FindByEmailAsync(loginRequest.Email)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.CheckPasswordAsync(user, loginRequest.Password)).ReturnsAsync(true);
            _userRepositoryMock.Setup(r => r.GetRolesAsync(user)).ReturnsAsync(roles);
            _jwtServiceMock.Setup(j => j.GenerateToken(user, roles)).Returns("jwt-token");
            _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AppUser>())).Returns(Task.CompletedTask);

            // Act
            var result = await _identityService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("jwt-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.NotEmpty(result.CsrfToken);
            Assert.NotNull(result.AuthResponse);
            Assert.Equal(user.UserName, result.AuthResponse.User.UserName);
            Assert.Equal(roles, result.AuthResponse.User.Roles);
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<AppUser>(u => u.RefreshToken == "refresh-token")), Times.Once());
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsAppException()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };
            _userRepositoryMock.Setup(r => r.FindByEmailAsync(loginRequest.Email)).ReturnsAsync((AppUser)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _identityService.LoginAsync(loginRequest));
            Assert.Equal(ErrorCode.INVALID_CREDENTIALS, exception.GetErrorCode());
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AppUser>()), Times.Never());
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsTokenResponse()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "refresh-token" };
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "TestUser",
                RefreshToken = refreshTokenRequest.RefreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            var roles = new List<string> { "User" };
            _userRepositoryMock.Setup(r => r.GetAllAsync())
                .Returns(new List<AppUser> { user }.AsQueryable());
            _userRepositoryMock.Setup(r => r.GetRolesAsync(user)).ReturnsAsync(roles);
            _jwtServiceMock.Setup(j => j.GenerateToken(user, roles)).Returns("new-jwt-token");
            _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");
            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AppUser>())).Returns(Task.CompletedTask);

            // Act
            var result = await _identityService.RefreshTokenAsync(refreshTokenRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new-jwt-token", result.AccessToken);
            Assert.Equal("new-refresh-token", result.RefreshToken);
            Assert.NotEmpty(result.CsrfToken);
            Assert.NotNull(result.AuthResponse);
            Assert.Equal(user.UserName, result.AuthResponse.User.UserName);
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<AppUser>(u => u.RefreshToken == "new-refresh-token")), Times.Once());
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ThrowsAppException()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "expired-token" };
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                RefreshToken = refreshTokenRequest.RefreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };
            _userRepositoryMock.Setup(r => r.GetAllAsync())
                .Returns(new List<AppUser> { user }.AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _identityService.RefreshTokenAsync(refreshTokenRequest));
            Assert.Equal(ErrorCode.INVALID_OR_EXPIRED_REFRESH_TOKEN, exception.GetErrorCode());
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AppUser>()), Times.Never());
        }

        [Fact]
        public async Task RegisterAsync_WithValidRequest_ReturnsTokenResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                UserName = "NewUser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                ImageUrl = "http://example.com/image.jpg",
                PublicId = "public-id"
            };
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = registerRequest.Email,
                UserName = registerRequest.UserName,
                ImageUrl = registerRequest.ImageUrl,
                PublicId = registerRequest.PublicId,
                CreatedDate = DateTime.UtcNow
            };
            var roles = new List<string> { "User" };
            _userRepositoryMock.Setup(r => r.FindByEmailAsync(registerRequest.Email)).ReturnsAsync((AppUser)null);
            _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<AppUser>(), registerRequest.Password)).ReturnsAsync(true);
            _userRepositoryMock.Setup(r => r.AddToRoleAsync(It.IsAny<AppUser>(), "User")).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(r => r.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(roles);
            _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<AppUser>(), roles)).Returns("jwt-token");
            _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AppUser>())).Returns(Task.CompletedTask);

            // Act
            var result = await _identityService.RegisterAsync(registerRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("jwt-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.NotEmpty(result.CsrfToken);
            Assert.NotNull(result.AuthResponse);
            Assert.Equal(registerRequest.UserName, result.AuthResponse.User.UserName);
            Assert.Equal(roles, result.AuthResponse.User.Roles);
            _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AppUser>(), registerRequest.Password), Times.Once());
            _userRepositoryMock.Verify(r => r.AddToRoleAsync(It.IsAny<AppUser>(), "User"), Times.Once());
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<AppUser>(u => u.RefreshToken == "refresh-token")), Times.Once());
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ThrowsAppException()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "existing@example.com",
                UserName = "NewUser",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };
            var existingUser = new AppUser { Email = registerRequest.Email };
            _userRepositoryMock.Setup(r => r.FindByEmailAsync(registerRequest.Email)).ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _identityService.RegisterAsync(registerRequest));
            Assert.Equal(ErrorCode.DUPLICATE_EMAIL, exception.GetErrorCode());
            _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task RegisterAsync_WithMismatchedPasswords_ThrowsAppException()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                UserName = "NewUser",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword123!"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _identityService.RegisterAsync(registerRequest));
            Assert.Equal(ErrorCode.PASSWORDS_DO_NOT_MATCH, exception.GetErrorCode());
            _userRepositoryMock.Verify(r => r.FindByEmailAsync(It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithValidUserId_ReturnsAuthResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "TestUser", Email = "test@example.com" };
            var roles = new List<string> { "User" };
            _userRepositoryMock.Setup(r => r.GetAllAsync())
                .Returns(new List<AppUser> { user }.AsQueryable());
            _userRepositoryMock.Setup(r => r.GetRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var result = await _identityService.GetCurrentUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Equal(user.UserName, result.User.UserName);
            Assert.Equal(roles, result.User.Roles);
            Assert.Equal("", result.CsrfToken); // CsrfToken is set to empty string in service
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithInvalidUserId_ThrowsAppException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetAllAsync())
                .Returns(new List<AppUser>().AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _identityService.GetCurrentUserAsync(userId));
            Assert.Equal(ErrorCode.USER_NOT_FOUND, exception.GetErrorCode());
        }
    }
}