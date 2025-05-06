using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using System.Threading.Tasks;

namespace EcommerceNashApp.Web.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(OrderRequest request);
        Task<OrderResponse> GetOrderAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
    }
}