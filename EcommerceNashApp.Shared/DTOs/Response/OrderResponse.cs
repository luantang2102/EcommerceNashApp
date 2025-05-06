namespace EcommerceNashApp.Shared.DTOs.Response
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid UserProfileId { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderItemResponse> OrderItems { get; set; } = [];
    }
}