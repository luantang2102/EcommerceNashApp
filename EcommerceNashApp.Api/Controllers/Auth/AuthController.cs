﻿using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers.Auth
{
    public class AuthController : BaseApiController
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var token = await _identityService.LoginAsync(loginRequest);
            return Ok(new ApiResponse<AuthResponse>(200, "Login successful", token));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            var result = await _identityService.RefreshTokenAsync(refreshTokenRequest);
            return Ok(new ApiResponse<AuthResponse>(200, "Token refreshed successfully", result));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var result = await _identityService.RegisterAsync(registerRequest);
            return CreatedAtAction(nameof(Register), new { id = result.User.Id },
                new ApiResponse<AuthResponse>(201, "User registered successfully", result));
        }
    }
}
