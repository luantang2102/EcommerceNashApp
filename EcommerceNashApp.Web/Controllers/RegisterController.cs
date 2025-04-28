using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Web.Models.DTOs.Request;
using EcommerceNashApp.Web.Models.DTOs;

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
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(null);
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
                    SetAuthCookies(result.Body);
                    returnUrl = returnUrl ?? Url.Action("Index", "Home");
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

        private void SetAuthCookies(AuthDto authResponse)
        {
            Response.Cookies.Append("jwt", authResponse.Jwt, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
            Response.Cookies.Append("refresh", authResponse.RefreshToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
            Response.Cookies.Append("csrf", authResponse.CsrfToken, new CookieOptions { HttpOnly = false, Secure = true, SameSite = SameSiteMode.Strict });
        }
    }
}