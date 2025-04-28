using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Web.Models.DTOs.Request;
using EcommerceNashApp.Web.Models.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace EcommerceNashApp.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RegisterController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _configuration = configuration;
        }

        // GET: /Register
        [HttpGet]
        public IActionResult Index(string returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(null);
        }

        // POST: /Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] RegisterRequest model, string returnUrl = "")
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

                var response = await _httpClient.PostAsync("/api/Auth/register", formData);
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

                    returnUrl = string.IsNullOrEmpty(returnUrl) ? Url.Action("Index", "Home")! : returnUrl;
                    return Redirect(returnUrl);
                }

                ModelState.AddModelError("", result?.Message ?? "Email đã được sử dụng.");
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