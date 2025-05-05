using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Api.Filters;
using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces.IServices.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EcommerceNashApp.Api.Controllers.Auth
{
    public class AuthController : BaseApiController
    {
        private readonly IIdentityService _identityService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IIdentityService identityService, IHttpContextAccessor httpContextAccessor, ILogger<AuthController> logger)
        {
            _identityService = identityService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void SetAuthCookies(string accessToken, string refreshToken, string csrfToken)
        {
            _logger.LogDebug("Setting auth_jwt cookie: {Token}", accessToken.Substring(0, Math.Min(20, accessToken.Length)) + "...");
            if (!accessToken.Contains("."))
            {
                _logger.LogError("Attempting to set malformed JWT in auth_jwt cookie: {Token}", accessToken);
                throw new InvalidOperationException("Cannot set malformed JWT in cookie");
            }

            var jwtCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(3),
                Path = "/"
            };
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("auth_jwt", accessToken, jwtCookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh", refreshToken, refreshCookieOptions);

            var csrfCookieOptions = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(3),
                Path = "/"
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("csrf", csrfToken, csrfCookieOptions);
        }

        [HttpPost("login")]
        [SkipCsrfValidation]
        public async Task<IActionResult> Login([FromForm] LoginRequest loginRequest)
        {
            var tokenResponse = await _identityService.LoginAsync(loginRequest);
            _logger.LogInformation("Login successful for user: {Email}", loginRequest.Email);
            SetAuthCookies(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.CsrfToken);
            return Ok(new ApiResponse<AuthResponse>(200, "Login successful", tokenResponse.AuthResponse));
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token missing in cookies");
                return BadRequest(new ApiResponse<string>(400, "Refresh token is missing", string.Empty));
            }

            var tokenResponse = await _identityService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });
            _logger.LogInformation("Token refreshed successfully");
            SetAuthCookies(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.CsrfToken);
            return Ok(new ApiResponse<AuthResponse>(200, "Token refreshed successfully", tokenResponse.AuthResponse));
        }

        [HttpPost("register")]
        [SkipCsrfValidation]
        public async Task<IActionResult> Register([FromForm] RegisterRequest registerRequest)
        {
            var tokenResponse = await _identityService.RegisterAsync(registerRequest);
            _logger.LogInformation("User registered: {Email}", registerRequest.Email);
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
            if (_httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue("csrf", out var csrfToken))
            {
                authResponse.CsrfToken = csrfToken;
            }
            _logger.LogInformation("User authenticated: {UserId}", userId);
            return Ok(new ApiResponse<AuthResponse>(200, "User authenticated", authResponse));
        }

        [HttpPost("logout")]
        [SkipCsrfValidation]
        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete("auth_jwt");
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("refresh");
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("csrf");
            _logger.LogInformation("User logged out");
            return Ok(new ApiResponse<string>(200, "Logged out successfully", string.Empty));
        }
    }
}