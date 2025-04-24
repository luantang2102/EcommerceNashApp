namespace EcommerceNashApp.Web.Models.DTOs
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public required string ImageUrl { get; set; }
        public required string PublicId { get; set; }
        public bool IsMain { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
