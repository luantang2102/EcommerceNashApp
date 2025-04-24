namespace EcommerceNashApp.Web.Models.Views
{
    public class ProductImageView
    {
        public Guid Id { get; set; }
        public required string ImageUrl { get; set; }
        public required string PublicId { get; set; }
        public bool IsMain { get; set; }
    }
}
