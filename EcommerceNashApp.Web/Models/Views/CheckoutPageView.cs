using EcommerceNashApp.Web.Models.DTOs;

namespace EcommerceNashApp.Web.Models.Views
{
    public class CheckoutPageView
    {
        public CartDto Cart { get; set; }
        public double DeliveryFee => 50000; // Matches StripeService
        public double Subtotal => Cart?.CartItems.Sum(item => item.Price * item.Quantity) ?? 0;
        public double Total => Subtotal + DeliveryFee;
    }
}