using EcommerceNashApp.Web.Models;
using EcommerceNashApp.Web.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EcommerceNashApp.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IHttpClientFactory httpClientFactory, ILogger<LoginController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string email, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var client = _httpClientFactory.CreateClient("NashApp.Api");
            var formContent = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent(password), "Password" }
            };

            _logger.LogInformation("Calling /api/Auth/login for email: {Email}", email);
            var response = await client.PostAsync("/api/Auth/login", formContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiDto<AuthDto>>(responseContent);
                _logger.LogInformation("Login successful for user: {UserId}", apiResponse.Body.User.Id);

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
                    _logger.LogWarning("No Set-Cookie headers found in /api/Auth/login response");
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
            _logger.LogError("Login failed: StatusCode={StatusCode}, Response={Content}", response.StatusCode, errorContent);
            ViewData["Error"] = "Invalid email or password.";
            return View();
        }

        // Helper to check authentication state
        public bool IsAuthenticated()
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            var isAuthenticated = !string.IsNullOrEmpty(userInfoJson);
            _logger.LogDebug("Checked authentication state: {IsAuthenticated}", isAuthenticated);
            return isAuthenticated;
        }

        // Helper to get user info
        public dynamic GetUserInfo()
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (!string.IsNullOrEmpty(userInfoJson))
            {
                return JsonConvert.DeserializeObject<dynamic>(userInfoJson);
            }
            _logger.LogWarning("No user info found in session");
            return null;
        }
    }
}