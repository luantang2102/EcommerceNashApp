using EcommerceNashApp.Web.Models.Views;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using EcommerceNashApp.Web.Configurations;
using EcommerceNashApp.Web.Services;
using Stripe;
using System.Text.Json;

namespace EcommerceNashApp.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<CheckoutController> _logger;
        private readonly StripeConfig _stripeConfig;

        public CheckoutController(
            ICartService cartService,
            IPaymentService paymentService,
            ILogger<CheckoutController> logger,
            IOptions<StripeConfig> stripeConfig)
        {
            _cartService = cartService;
            _paymentService = paymentService;
            _logger = logger;
            _stripeConfig = stripeConfig.Value;
        }

        private bool IsAuthenticated()
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            var isAuthenticated = !string.IsNullOrEmpty(userInfoJson);
            _logger.LogDebug("Checked authentication state: {IsAuthenticated}", isAuthenticated);
            return isAuthenticated;
        }

        [HttpGet]
        public IActionResult Config()
        {
            if (string.IsNullOrEmpty(_stripeConfig.PublishableKey))
            {
                _logger.LogError("Stripe PublishableKey is not configured");
                return StatusCode(500, new { message = "Cấu hình Stripe không hợp lệ." });
            }
            return Json(new { publishableKey = _stripeConfig.PublishableKey });
        }

        [HttpPost]
        [Route("create-payment-intent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentIntent()
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated for creating payment intent");
                return StatusCode(401, new { message = "Vui lòng đăng nhập để tiếp tục thanh toán." });
            }

            try
            {
                var clientSecret = await _paymentService.CreateOrUpdatePaymentIntentAsync();
                if (string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("Payment intent creation returned empty client secret");
                    return StatusCode(500, new { message = "Không thể tạo phiên thanh toán." });
                }

                _logger.LogInformation("Payment intent created successfully");
                return Json(new { clientSecret });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authentication failed for payment intent creation");
                return StatusCode(401, new { message = "Vui lòng đăng nhập lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating payment intent");
                return StatusCode(500, new { message = "Không thể tạo phiên thanh toán. Vui lòng thử lại." });
            }
        }

        public async Task<IActionResult> Index(int step = 1, string session_id = null, bool succeeded = false, string paymentMessage = null)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Checkout") });
            }

            try
            {
                var cart = await _cartService.GetCartAsync();
                if (cart == null || !cart.CartItems.Any())
                {
                    _logger.LogWarning("Cart is empty or null");
                    TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                    return RedirectToAction("Index", "Cart");
                }

                if (step < 1 || step > 4)
                {
                    _logger.LogWarning("Invalid step value: {Step}, defaulting to 1", step);
                    step = 1;
                }

                var model = new CheckoutView
                {
                    CurrentStep = step,
                    Cart = new CartPageView
                    {
                        Items = cart.CartItems.Select(i => new CartItemResponse
                        {
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            Price = i.Price,
                            Quantity = i.Quantity
                        }).ToList()
                    },
                    SessionId = session_id,
                    PaymentSucceeded = succeeded,
                    PaymentMessage = paymentMessage,
                    ShippingAddress = new ShippingAddressView()
                };

                if (step == 1)
                {
                    var sessionAddressJson = HttpContext.Session.GetString("TempShippingAddress");
                    if (!string.IsNullOrEmpty(sessionAddressJson))
                    {
                        model.ShippingAddress = JsonConvert.DeserializeObject<ShippingAddressView>(sessionAddressJson);
                        _logger.LogInformation("Loaded temporary address from session for step 1");
                    }
                }
                else if (step == 2 || step == 3)
                {
                    var sessionAddressJson = HttpContext.Session.GetString("TempShippingAddress");
                    if (!string.IsNullOrEmpty(sessionAddressJson))
                    {
                        model.ShippingAddress = JsonConvert.DeserializeObject<ShippingAddressView>(sessionAddressJson);
                        _logger.LogInformation("Loaded temporary address from session for step {Step}", step);
                    }
                    else
                    {
                        _logger.LogWarning("No address found for step {Step}, redirecting to step 1", step);
                        TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng.";
                        return RedirectToAction("Index", new { step = 1 });
                    }
                }
                else if (step == 4)
                {
                    _logger.LogInformation("Rendering step 4 with session_id: {SessionId}, succeeded: {Succeeded}, message: {Message}", session_id, succeeded, paymentMessage);
                    model.PaymentSucceeded = succeeded;
                    model.PaymentMessage = paymentMessage;
                    var sessionAddressJson = HttpContext.Session.GetString("TempShippingAddress");
                    if (!string.IsNullOrEmpty(sessionAddressJson))
                    {
                        model.ShippingAddress = JsonConvert.DeserializeObject<ShippingAddressView>(sessionAddressJson);
                    }
                }

                ViewBag.StripePublishableKey = _stripeConfig.PublishableKey;
                _logger.LogInformation("Rendering Index with step: {Step}, Address: {Address}", model.CurrentStep, model.ShippingAddress?.FullName ?? "None");
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authentication failed, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Checkout") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["Error"] = "Không thể tải trang thanh toán. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        [Route("Return")]
        public async Task<IActionResult> Return(string payment_intent, string payment_intent_client_secret)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Checkout") });
            }

            try
            {
                if (string.IsNullOrEmpty(payment_intent))
                {
                    _logger.LogWarning("No payment_intent provided in Return URL");
                    return RedirectToAction("Index", new { step = 3, paymentMessage = "Không tìm thấy thông tin thanh toán." });
                }

                _logger.LogInformation("Handling payment return for payment_intent: {PaymentIntentId}", payment_intent);

                // Verify the payment status using Stripe API
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(payment_intent);

                if (paymentIntent.Status == "succeeded")
                {
                    _logger.LogInformation("Payment succeeded for payment_intent: {PaymentIntentId}", payment_intent);

                    // Call CompleteOrder to finalize the order and clear the cart
                    var model = new CheckoutView
                    {
                        ShippingAddress = new ShippingAddressView(),
                        SessionId = payment_intent_client_secret
                    };
                    var sessionAddressJson = HttpContext.Session.GetString("TempShippingAddress");
                    if (!string.IsNullOrEmpty(sessionAddressJson))
                    {
                        model.ShippingAddress = JsonConvert.DeserializeObject<ShippingAddressView>(sessionAddressJson);
                    }

                    var completeOrderResult = await CompleteOrder(model);
                    if (completeOrderResult is JsonResult jsonResult && jsonResult.Value is JsonElement jsonElement &&
                        jsonElement.TryGetProperty("success", out JsonElement successProperty) && successProperty.GetBoolean())
                    {
                        _logger.LogInformation("Order completed successfully for payment_intent: {PaymentIntentId}", payment_intent);
                        return RedirectToAction("Index", new { step = 4, session_id = payment_intent_client_secret, succeeded = true, paymentMessage = "Thanh toán thành công!" });
                    }
                    else
                    {
                        _logger.LogError("Failed to complete order after successful payment");
                        return RedirectToAction("Index", new { step = 4, session_id = payment_intent_client_secret, succeeded = false, paymentMessage = "Thanh toán thành công nhưng không thể hoàn tất đơn hàng." });
                    }
                }
                else
                {
                    _logger.LogWarning("Payment did not succeed for payment_intent: {PaymentIntentId}, status: {Status}", payment_intent, paymentIntent.Status);
                    return RedirectToAction("Index", new { step = 4, session_id = payment_intent_client_secret, succeeded = false, paymentMessage = "Thanh toán không thành công. Vui lòng thử lại." });
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error while verifying payment_intent: {PaymentIntentId}", payment_intent);
                return RedirectToAction("Index", new { step = 4, session_id = payment_intent_client_secret, succeeded = false, paymentMessage = "Lỗi xác minh thanh toán. Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling payment return for payment_intent: {PaymentIntentId}", payment_intent);
                return RedirectToAction("Index", new { step = 4, session_id = payment_intent_client_secret, succeeded = false, paymentMessage = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NextStep(CheckoutView model)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Checkout") });
            }

            try
            {
                var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                _logger.LogInformation("NextStep form data: {FormData}", string.Join(", ", formData.Select(kv => $"{kv.Key}={kv.Value}")));

                int currentStep = model.CurrentStep;
                if (Request.Form.ContainsKey("CurrentStep") && int.TryParse(Request.Form["CurrentStep"], out int formStep))
                {
                    currentStep = formStep;
                    _logger.LogInformation("CurrentStep from form: {FormStep}", formStep);
                }
                else
                {
                    _logger.LogWarning("CurrentStep not found, using model value: {ModelStep}", model.CurrentStep);
                }

                if (currentStep < 1 || currentStep > 3)
                {
                    _logger.LogWarning("Invalid CurrentStep: {CurrentStep}, defaulting to 1", currentStep);
                    currentStep = 1;
                }

                model.CurrentStep = currentStep;

                if (model.CurrentStep == 1)
                {
                    if (model.ShippingAddress == null || string.IsNullOrEmpty(model.ShippingAddress.FullName))
                    {
                        _logger.LogWarning("ShippingAddress is invalid in step 1");
                        TempData["Error"] = "Vui lòng nhập đầy đủ thông tin địa chỉ.";
                        model.Cart = await GetCartItems();
                        return View("Index", model);
                    }

                    _logger.LogInformation("Binding successful. ShippingAddress: FullName={FullName}, Address1={Address1}",
                        model.ShippingAddress.FullName, model.ShippingAddress.Address1);

                    HttpContext.Session.SetString("TempShippingAddress", JsonConvert.SerializeObject(model.ShippingAddress));
                    _logger.LogInformation("Stored shipping address in session: {Address}", JsonConvert.SerializeObject(model.ShippingAddress));
                    model.CurrentStep = 2;
                }
                else if (model.CurrentStep == 2)
                {
                    model.CurrentStep = 3;
                }
                else if (model.CurrentStep == 3)
                {
                    model.CurrentStep = 4;
                }

                _logger.LogInformation("Redirecting to Index with step: {Step}, Address in session: {Address}",
                    model.CurrentStep, HttpContext.Session.GetString("TempShippingAddress") ?? "None");
                return RedirectToAction("Index", new { step = model.CurrentStep, sessionId = model.SessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout step");
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return RedirectToAction("Index", new { step = model.CurrentStep });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreviousStep(CheckoutView model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Checkout") });
            }

            try
            {
                int currentStep = model.CurrentStep;
                if (Request.Form.ContainsKey("CurrentStep") && int.TryParse(Request.Form["CurrentStep"], out int formStep))
                {
                    currentStep = formStep;
                    _logger.LogInformation("CurrentStep from form: {FormStep}", formStep);
                }
                else
                {
                    _logger.LogWarning("CurrentStep not found, using model value: {ModelStep}", model.CurrentStep);
                }

                if (currentStep < 1 || currentStep > 3)
                {
                    _logger.LogWarning("Invalid CurrentStep: {CurrentStep}, defaulting to 1", currentStep);
                    currentStep = 1;
                }
                else
                {
                    currentStep = currentStep > 1 ? currentStep - 1 : 1;
                }

                model.CurrentStep = currentStep;

                _logger.LogInformation("Redirecting to Index with step: {Step}", model.CurrentStep);
                return RedirectToAction("Index", new { step = model.CurrentStep });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing previous step");
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return RedirectToAction("Index", new { step = model.CurrentStep });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(CheckoutView model)
        {
            if (!IsAuthenticated())
            {
                return StatusCode(401, new { message = "Vui lòng đăng nhập." });
            }

            try
            {
                var orderRequest = new OrderRequest
                {
                    SaveAddress = model.ShippingAddress.SaveAddress,
                    ShippingAddress = new ShippingAddressRequest
                    {
                        FullName = model.ShippingAddress.FullName,
                        Address1 = model.ShippingAddress.Address1,
                        Address2 = model.ShippingAddress.Address2,
                        City = model.ShippingAddress.City,
                        State = model.ShippingAddress.State,
                        Zip = model.ShippingAddress.Zip,
                        Country = model.ShippingAddress.Country
                    }
                };

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsJsonAsync("https://localhost:5001/api/Order/create", orderRequest);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create order: {StatusCode}, {Error}", response.StatusCode, errorContent);
                    return StatusCode(500, new { message = "Không thể tạo đơn hàng. Vui lòng thử lại." });
                }

                await _cartService.ClearCartAsync();
                HttpContext.Session.Remove("TempShippingAddress");
                _logger.LogInformation("Order completed: Cart cleared, TempShippingAddress removed");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing order");
                return StatusCode(500, new { message = "Không thể hoàn tất đơn hàng. Vui lòng thử lại." });
            }
        }

        private async Task<CartPageView> GetCartItems()
        {
            try
            {
                var cart = await _cartService.GetCartAsync();
                return new CartPageView
                {
                    Items = cart.CartItems.Select(i => new CartItemResponse
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch cart items");
                return new CartPageView { Items = new List<CartItemResponse>() };
            }
        }
    }
}