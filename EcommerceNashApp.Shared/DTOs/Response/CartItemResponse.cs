namespace EcommerceNashApp.Shared.DTOs.Response
{
    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<ProductImageResponse> Images { get; set; } = [];
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
