using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IOrderService
    {
        Task CreateOrderAsync(Guid userId, bool saveAddress, ShippingAddressRequest shippingAddress);
        Task<OrderResponse> GetOrderByIdAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
    }
}
