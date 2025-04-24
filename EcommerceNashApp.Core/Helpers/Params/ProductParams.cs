using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Infrastructure.Helpers.Params
{
    public class ProductParams : PaginationParams
    {
        public string? OrderBy { get; set; }
        public string? SearchTerm { get; set; }
        public string? Categories { get; set; }
        public string? Ratings { get; set; }
        public string? MinPrice { get; set; }
        public string? MaxPrice { get; set; }
    }
}
