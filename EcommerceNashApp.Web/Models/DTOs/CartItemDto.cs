namespace EcommerceNashApp.Web.Models.DTOs
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<ProductImageDto> Images { get; set; } = [];
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
