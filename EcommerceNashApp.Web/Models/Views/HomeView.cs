namespace EcommerceNashApp.Web.Models.Views
{
    public class HomeView
    {
        public required List<ProductView> Products { get; set; }
        public required List<CategoryView> Categories { get; set; }
    }
}
