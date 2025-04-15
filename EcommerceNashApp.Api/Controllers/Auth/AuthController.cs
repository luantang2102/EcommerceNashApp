using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Core.DTOs.Auth.Request;
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
            if (token == null)
                return Unauthorized("Invalid credentials");

            return Ok(new { token });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            try
            {
                var result = await _identityService.RefreshTokenAsync(refreshTokenRequest);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
