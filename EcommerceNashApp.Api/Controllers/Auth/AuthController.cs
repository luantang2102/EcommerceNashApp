using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Api.Filters;
using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces.IServices.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers.Auth
{
    public class AuthController : BaseApiController
    {
        private readonly IIdentityService _identityService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IIdentityService identityService, IHttpContextAccessor httpContextAccessor)
        {
            _identityService = identityService;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthCookies(string accessToken, string refreshToken, string csrfToken)
        {
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
        }

        [HttpPost("login")]
        [SkipCsrfValidation]
        public async Task<IActionResult> Login([FromForm] LoginRequest loginRequest)
        {
            var tokenResponse = await _identityService.LoginAsync(loginRequest);
            SetAuthCookies(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.CsrfToken);
            return Ok(new ApiResponse<AuthResponse>(200, "Login successful", tokenResponse.AuthResponse));
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new ApiResponse<string>(400, "Refresh token is missing", string.Empty));
            }

            var tokenResponse = await _identityService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });
            SetAuthCookies(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.CsrfToken);
            return Ok(new ApiResponse<AuthResponse>(200, "Token refreshed successfully", tokenResponse.AuthResponse));
        }

        [HttpPost("register")]
        [SkipCsrfValidation]
        public async Task<IActionResult> Register([FromForm] RegisterRequest registerRequest)
        {
            var tokenResponse = await _identityService.RegisterAsync(registerRequest);
            SetAuthCookies(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.CsrfToken);
            return CreatedAtAction(nameof(Register), new { id = tokenResponse.AuthResponse.User.Id },
                new ApiResponse<AuthResponse>(201, "User registered successfully", tokenResponse.AuthResponse));
        }

        [HttpGet("check")]
        [SkipCsrfValidation]
        public async Task<IActionResult> CheckAuth()
        {
            var userId = User.GetUserId();
            var authResponse = await _identityService.GetCurrentUserAsync(userId);
            authResponse.CsrfToken = _httpContextAccessor.HttpContext.Request.Cookies["csrf"];
            return Ok(new ApiResponse<AuthResponse>(200, "User authenticated", authResponse));
        }

        [HttpPost("logout")]
        [SkipCsrfValidation]
        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("jwt");
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("refresh");
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("csrf");
            return Ok(new ApiResponse<string>(200, "Logged out successfully", string.Empty));
        }
    }
}