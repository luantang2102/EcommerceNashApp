using EcommerceNashApp.Shared.DTOs.Auth.Request;
using EcommerceNashApp.Shared.DTOs.Auth.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using EcommerceNashApp.Web.Models.OverrideValidation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace EcommerceNashApp.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] RegisterRequestCli model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                TempData["Error"] = "Mật khẩu và xác nhận mật khẩu không khớp.";
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

                var response = await client.PostAsync("/api/Auth/register", formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<AuthResponse>>(responseContent);

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

                    // Store user info in session
                    var userInfo = new
                    {
                        UserId = apiResponse.Body.User.Id,
                        UserName = apiResponse.Body.User.UserName,
                        Roles = apiResponse.Body.User.Roles
                    };
                    HttpContext.Session.SetString("UserInfo", JsonConvert.SerializeObject(userInfo));

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ApiResponse<AuthResponse>>(errorContent);
                TempData["Error"] = errorResponse?.Message ?? "Email đã được sử dụng.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.";
                return View(model);
            }
        }
    }

}