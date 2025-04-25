using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices.Auth;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services.Auth
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityService(IUserRepository userRepository, IJwtService jwt, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _jwt = jwt;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userRepository.FindByEmailAsync(loginRequest.Email);
            if (user == null || !await _userRepository.CheckPasswordAsync(user, loginRequest.Password))
            {
                var attributes = new Dictionary<string, object>
                {
                    { "email", loginRequest.Email }
                };
                throw new AppException(ErrorCode.INVALID_CREDENTIALS, attributes);
            }

            var roles = await _userRepository.GetRolesAsync(user);
            var accessToken = _jwt.GenerateToken(user, roles);
            var refreshToken = _jwt.GenerateRefreshToken();
            var csrfToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            // Set JWT cookie
            var jwtCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1) // Match access token expiration
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("jwt", accessToken, jwtCookieOptions);

            // Set refresh token cookie
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // Match refresh token expiration
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh", refreshToken, refreshCookieOptions);

            // Set CSRF cookie
            var csrfCookieOptions = new CookieOptions
            {
                HttpOnly = false, // Accessible by JavaScript
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("csrf", csrfToken, csrfCookieOptions);

            return new AuthResponse
            {
                CsrfToken = csrfToken,
                User = user.MapModelToResponse(_userRepository.GetRolesAsync(user).Result)
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto)
        {
            var user = await _userRepository.GetAllAsync()
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "refreshToken", dto.RefreshToken }
                };
                throw new AppException(ErrorCode.INVALID_OR_EXPIRED_REFRESH_TOKEN, attributes);
            }

            var roles = await _userRepository.GetRolesAsync(user);
            var newAccessToken = _jwt.GenerateToken(user, roles);
            var newRefreshToken = _jwt.GenerateRefreshToken();
            var newCsrfToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            // Set new JWT cookie
            var jwtCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("jwt", newAccessToken, jwtCookieOptions);

            // Set new refresh token cookie
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh", newRefreshToken, refreshCookieOptions);

            // Set new CSRF cookie
            var csrfCookieOptions = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("csrf", newCsrfToken, csrfCookieOptions);

            return new AuthResponse
            {
                CsrfToken = newCsrfToken,
                User = user.MapModelToResponse(_userRepository.GetRolesAsync(user).Result)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "password", registerRequest.Password },
                    { "confirmPassword", registerRequest.ConfirmPassword }
                };
                throw new AppException(ErrorCode.PASSWORDS_DO_NOT_MATCH, attributes);
            }

            var existingUser = await _userRepository.FindByEmailAsync(registerRequest.Email);
            if (existingUser != null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "email", registerRequest.Email }
                };
                throw new AppException(ErrorCode.DUPLICATE_EMAIL, attributes);
            }

            var user = new AppUser
            {
                UserName = registerRequest.Email,
                Email = registerRequest.Email,
                ImageUrl = registerRequest.ImageUrl,
                PublicId = registerRequest.PublicId,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userRepository.CreateAsync(user, registerRequest.Password);
            if (!result)
            {
                var errors = new List<string> { "User creation failed." };
                var attributes = new Dictionary<string, object>

                {
                    { "errors", errors }
                };
                throw new AppException(ErrorCode.IDENTITY_CREATION_FAILED, attributes);
            }

            await _userRepository.AddToRoleAsync(user, "User");

            var roles = await _userRepository.GetRolesAsync(user);
            var accessToken = _jwt.GenerateToken(user, roles);
            var refreshToken = _jwt.GenerateRefreshToken();
            var csrfToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            var jwtCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("jwt", accessToken, jwtCookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh", refreshToken, refreshCookieOptions);

            var csrfCookieOptions = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("csrf", csrfToken, csrfCookieOptions);

            return new AuthResponse
            {
                CsrfToken = csrfToken,
                User = user.MapModelToResponse(_userRepository.GetRolesAsync(user).Result)
            };
        }

        public async Task<AuthResponse> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userRepository.GetAllAsync().FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                var attribute = new Dictionary<string, object>
                {
                    { "userId", userId }
                };
                throw new AppException(ErrorCode.USER_NOT_FOUND, attribute);
            }

            return new AuthResponse
            {
                User = user.MapModelToResponse(_userRepository.GetRolesAsync(user).Result),
                CsrfToken = _httpContextAccessor.HttpContext.Request.Cookies["csrf"]
            };
        }
    }
}