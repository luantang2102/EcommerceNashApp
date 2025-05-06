namespace EcommerceNashApp.Shared.DTOs.Request
{
    public class CartItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
