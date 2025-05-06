using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("create")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            var userId = User.GetUserId();
            await _orderService.CreateOrderAsync(userId, request.SaveAddress, request.ShippingAddress);
            return Ok(new ApiResponse<string>(200, "Order created successfully", "Created"));
        }
    }

    
}