using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("me/intent")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> CreateOrUpdatePaymentIntent()
        {
            var userId = User.GetUserId();
            var cliSecret = await _paymentService.CreateOrUpdatePaymentIntentAsync(userId) ?? "";
            return Ok(new ApiResponse<string>(200, "Payment intent created or updated successfully", cliSecret));
        }
    }
}