using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EcommerceNashApp.Api.Controllers
{
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("intent")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> CreateOrUpdatePaymentIntent()
        {
            var userId = User.GetUserId();
            await _paymentService.CreateOrUpdatePaymentIntentAsync(userId);
            return Ok(new ApiResponse<string>(200, "Payment intent created or updated successfully", "created/updated"));
        }
    }
}