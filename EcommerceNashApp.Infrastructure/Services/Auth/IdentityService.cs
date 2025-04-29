using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices.Auth;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services.Auth
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwt;

        public IdentityService(IUserRepository userRepository, IJwtService jwt)
        {
            _userRepository = userRepository;
            _jwt = jwt;
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest loginRequest)
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

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                CsrfToken = csrfToken,
                AuthResponse = new AuthResponse
                {
                    CsrfToken = csrfToken,
                    User = user.MapModelToResponse(roles)
                }
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest dto)
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

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                CsrfToken = newCsrfToken,
                AuthResponse = new AuthResponse
                {
                    CsrfToken = newCsrfToken,
                    User = user.MapModelToResponse(roles)
                }
            };
        }

        public async Task<TokenResponse> RegisterAsync(RegisterRequest registerRequest)
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
                UserName = registerRequest.UserName,
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

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                CsrfToken = csrfToken,
                AuthResponse = new AuthResponse
                {
                    CsrfToken = csrfToken,
                    User = user.MapModelToResponse(roles)
                }
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

            var roles = await _userRepository.GetRolesAsync(user);
            return new AuthResponse
            {
                User = user.MapModelToResponse(roles),
                CsrfToken = ""
            };
        }
    }
}