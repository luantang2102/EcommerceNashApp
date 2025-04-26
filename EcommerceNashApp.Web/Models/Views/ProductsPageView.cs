namespace EcommerceNashApp.Web.Models.Views
{
    public class ProductsPageView
    {
        public List<ProductView> Products { get; set; } = [];
        public List<CategoryView> Categories { get; set; } = [];
        public List<Guid> SelectedCategoryIds { get; set; } = [];
        public double? MinPrice { get; set; } = 0;
        public double? MaxPrice { get; set; } = 0;
        public string? SelectedMinPrice { get; set; }
        public string? SelectedMaxPrice { get; set; }
        public string? SortBy { get; set; }
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
    }
}