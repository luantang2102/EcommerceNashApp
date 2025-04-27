namespace EcommerceNashApp.Web.Models.Views
{
    public class ProductRatingView
    {
        public Guid Id { get; set; }
        public int Value { get; set; } 
        public string? Comment { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
