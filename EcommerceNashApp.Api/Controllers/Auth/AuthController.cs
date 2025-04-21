using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Filters;
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
        [SkipCsrfValidation]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var token = await _identityService.LoginAsync(loginRequest);
            return Ok(new ApiResponse<AuthResponse>(200, "Login successful", token));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new ApiResponse<string>(400, "Refresh token is missing", string.Empty));
            }

            var response = await _identityService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });
            return Ok(new ApiResponse<AuthResponse>(200, "Token refreshed successfully", response));
        }

        [HttpPost("register")]
        [SkipCsrfValidation]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var result = await _identityService.RegisterAsync(registerRequest);
            return CreatedAtAction(nameof(Register), new { id = result.User.Id },
                new ApiResponse<AuthResponse>(201, "User registered successfully", result));
        }

        [HttpPost("logout")]
        [SkipCsrfValidation]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            Response.Cookies.Delete("refresh");
            Response.Cookies.Delete("csrf");
            return Ok(new ApiResponse<string>(200, "Logged out successfully", string.Empty));
        }
    }
}
