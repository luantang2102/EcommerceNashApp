using EcommerceNashApp.Web.Models.DTOs;
using EcommerceNashApp.Web.Models.DTOs.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace EcommerceNashApp.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LoginController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _configuration = configuration;
            _logger = logger;
        }

        // GET: /Login
        [HttpGet]
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] LoginRequest model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(model.Email), "email" },
                    { new StringContent(model.Password), "password" },
                };

                _logger.LogInformation("Calling /api/Auth/login for email: {Email}", model.Email);
                var response = await _httpClient.PostAsync("/api/Auth/login", formData);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API response: {Content}", content);

                var result = JsonSerializer.Deserialize<ApiDto<AuthDto>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (response.IsSuccessStatusCode)
                {
                    // API sets jwt, refresh, and csrf cookies
                    _logger.LogInformation("Login successful for user ID: {UserId}", result.Body.User.Id);

                    // Create minimal claims for MVC authentication
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.Body.User.Id.ToString()),
                        new Claim(ClaimTypes.Email, result.Body.User.Email ?? string.Empty),
                        new Claim(ClaimTypes.Name, result.Body.User.UserName ?? "Anonymous"),
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(3) // Match jwt cookie
                    });

                    returnUrl = returnUrl ?? Url.Action("Index", "Home");
                    _logger.LogInformation("Redirecting to {ReturnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }

                _logger.LogWarning("Login failed: {Message}", result?.Message);
                ModelState.AddModelError("", result?.Message ?? "Email hoặc mật khẩu không đúng.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
                ModelState.AddModelError("", "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
                return View(model);
            }
        }
    }
}