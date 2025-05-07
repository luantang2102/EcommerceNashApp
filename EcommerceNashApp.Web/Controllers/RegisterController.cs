using EcommerceNashApp.Shared.DTOs.Auth.Request;
using EcommerceNashApp.Shared.DTOs.Auth.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace EcommerceNashApp.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IHttpClientFactory httpClientFactory, ILogger<RegisterController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

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
                var client = _httpClientFactory.CreateClient("NashApp.Api");
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(model.UserName), "UserName" },
                    { new StringContent(model.Email), "Email" },
                    { new StringContent(model.Password), "Password" },
                    { new StringContent(model.ConfirmPassword), "ConfirmPassword" }
                };
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    formData.Add(new StringContent(model.ImageUrl), "ImageUrl");
                }

                _logger.LogInformation("Calling /api/Auth/register for email: {Email}", model.Email);
                var response = await client.PostAsync("/api/Auth/register", formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<AuthResponse>>(responseContent);
                    _logger.LogInformation("Registration successful for user: {UserId}", apiResponse.Body.User.Id);

                    // Propagate API cookies to browser and CookieContainer
                    if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
                    {
                        if (client.GetType().GetProperty("Handler")?.GetValue(client) is HttpClientHandler handler && handler.UseCookies)
                        {
                            var cookieContainer = handler.CookieContainer;
                            foreach (var cookieHeader in setCookies)
                            {
                                var cookieParts = cookieHeader.Split(';');
                                var nameValue = cookieParts[0].Split('=');
                                if (nameValue.Length == 2)
                                {
                                    var cookie = new Cookie(nameValue[0].Trim(), nameValue[1].Trim())
                                    {
                                        Domain = "localhost",
                                        Path = "/",
                                        Secure = true,
                                        HttpOnly = cookieParts.Any(p => p.Trim().ToLower() == "httponly")
                                    };
                                    foreach (var part in cookieParts.Skip(1))
                                    {
                                        var kv = part.Split('=');
                                        if (kv.Length == 2 && kv[0].Trim().ToLower() == "expires")
                                        {
                                            if (DateTime.TryParse(kv[1].Trim(), out var expires))
                                                cookie.Expires = expires;
                                        }
                                    }
                                    cookieContainer.Add(new Uri("https://localhost:5001"), cookie);
                                    _logger.LogDebug("Added cookie to CookieContainer: {Name}={Value}", cookie.Name, cookie.Value.Substring(0, Math.Min(20, cookie.Value.Length)) + "...");
                                }
                                HttpContext.Response.Headers.Append("Set-Cookie", cookieHeader);
                            }
                        }
                        else
                        {
                            foreach (var cookie in setCookies)
                            {
                                HttpContext.Response.Headers.Append("Set-Cookie", cookie);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No Set-Cookie headers found in /api/Auth/register response");
                    }

                    // Store user info in session
                    var userInfo = new
                    {
                        UserId = apiResponse.Body.User.Id,
                        UserName = apiResponse.Body.User.UserName,
                        Roles = apiResponse.Body.User.Roles
                    };
                    HttpContext.Session.SetString("UserInfo", JsonConvert.SerializeObject(userInfo));
                    _logger.LogInformation("Stored user info in session for UserId: {UserId}", userInfo.UserId);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ApiResponse<AuthResponse>>(errorContent);
                _logger.LogWarning("Registration failed: {Message}", errorResponse?.Message);
                ModelState.AddModelError("", errorResponse?.Message ?? "Email đã được sử dụng.");
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