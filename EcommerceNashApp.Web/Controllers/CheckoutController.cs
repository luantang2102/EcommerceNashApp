using EcommerceNashApp.Web.Models.Views;
using EcommerceNashApp.Web.Services.Impl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EcommerceNashApp.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly CartService _apiService;

        public CheckoutController(CartService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await _apiService.GetCartAsync();
            var model = new CheckoutPageView
            {
                Cart = cart
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent()
        {
            await _apiService.CreateOrUpdatePaymentIntentAsync();
            var cart = await _apiService.GetCartAsync();
            var model = new CheckoutPageView
            {
                Cart = cart
            };
            return View("Index", model);
        }
    }
}