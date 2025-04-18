﻿using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.Auth;
using EcommerceNashApp.Core.Models.Identity;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services.Auth
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwt;

        public IdentityService(UserManager<AppUser> userManager, IJwtService jwt)
        {
            _userManager = userManager;
            _jwt = jwt;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                var attributes = new Dictionary<string, object>
                {
                    { "email", loginRequest.Email }
                };
                throw new AppException(ErrorCode.INVALID_CREDENTIALS, attributes);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwt.GenerateToken(user, roles);
            var refreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = user.MapModelToResponse()
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "refreshToken", dto.RefreshToken }
                };
                throw new AppException(ErrorCode.INVALID_OR_EXPIRED_REFRESH_TOKEN, attributes);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwt.GenerateToken(user, roles);
            var newRefreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = user.MapModelToResponse()
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

            var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
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
                Email = registerRequest.Email
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                var attributes = new Dictionary<string, object>
                {
                    { "errors", errors }
                };
                throw new AppException(ErrorCode.IDENTITY_CREATION_FAILED, attributes);
            }

            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwt.GenerateToken(user, roles);
            var refreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = user.MapModelToResponse()
            };
        }
    }
}
