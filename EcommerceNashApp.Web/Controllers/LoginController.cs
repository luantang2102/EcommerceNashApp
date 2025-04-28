using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using EcommerceNashApp.Web.Models.DTOs;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Web.Models.DTOs.Request;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EcommerceNashApp.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public LoginController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _configuration = configuration;
        }

        // GET: /Login
        [HttpGet]
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(null);
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

                var response = await _httpClient.PostAsync("/api/Auth/login", formData);
                var result = await response.Content.ReadFromJsonAsync<ApiDto<AuthDto>>();

                if (response.IsSuccessStatusCode)
                {
                    // Set csrf cookie
                    SetCsrfCookie(result.Body);

                    // Create claims for user info
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.Body.User.Id.ToString()),
                        new Claim(ClaimTypes.Email, result.Body.User.Email ?? string.Empty),
                        new Claim(ClaimTypes.Name, result.Body.User.UserName ?? "Anonymous"),
                    };

                    // Create identity and principal
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("CookieAuth", principal, new AuthenticationProperties
                    {
                        IsPersistent = false
                    });

                    returnUrl = returnUrl ?? Url.Action("Index", "Home");
                    return Redirect(returnUrl);
                }

                ModelState.AddModelError("", result?.Message ?? "Email hoặc mật khẩu không đúng.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        private void SetCsrfCookie(AuthDto authResponse)
        {
            Response.Cookies.Append("csrf", authResponse.CsrfToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}