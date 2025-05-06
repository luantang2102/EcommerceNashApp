using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Web.Models.Views
{
    public class CartPageView
    {
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
        public double Subtotal => Items.Sum(item => item.Price * item.Quantity);
    }
}