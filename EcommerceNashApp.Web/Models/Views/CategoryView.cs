namespace EcommerceNashApp.Web.Models.Views
{
    public class CategoryView
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public List<CategoryView> SubCategories { get; set; } = [];
        public Guid? ParentCategoryId { get; set; }
    }
}
