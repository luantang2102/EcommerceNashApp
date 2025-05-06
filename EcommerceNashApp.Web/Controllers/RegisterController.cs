using EcommerceNashApp.Shared.DTOs.Auth.Request;
using EcommerceNashApp.Shared.DTOs.Auth.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace EcommerceNashApp.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RegisterController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _configuration = configuration;
            _logger = logger;
        }

        // GET: /Register
        [HttpGet]
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] RegisterRequest model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu và xác nhận mật khẩu không khớp.");
                return View(model);
            }

            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(model.UserName), "userName" },
                    { new StringContent(model.Email), "email" },
                    { new StringContent(model.Password), "password" },
                    { new StringContent(model.ConfirmPassword), "confirmPassword" }
                };
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    formData.Add(new StringContent(model.ImageUrl), "imageUrl");
                }

                _logger.LogInformation("Calling /api/Auth/register for email: {Email}", model.Email);
                var response = await _httpClient.PostAsync("/api/Auth/register", formData);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API response: {Content}", content);

                var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (response.IsSuccessStatusCode)
                {
                    // API sets jwt, refresh, and csrf cookies
                    _logger.LogInformation("Registration successful for user ID: {UserId}", result.Body.User.Id);

                    // Create claims for MVC authentication
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

                    returnUrl = string.IsNullOrEmpty(returnUrl) ? Url.Action("Index", "Home") : returnUrl;
                    _logger.LogInformation("Redirecting to {ReturnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }

                _logger.LogWarning("Registration failed: {Message}", result?.Message);
                ModelState.AddModelError("", result?.Message ?? "Email đã được sử dụng.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", model.Email);
                ModelState.AddModelError("", "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
                return View(model);
            }
        }
    }
}