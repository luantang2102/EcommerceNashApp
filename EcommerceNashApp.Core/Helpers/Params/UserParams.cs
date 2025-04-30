using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Core.Helpers.Params
{
    public class UserParams : PaginationParams
    {
        public string? OrderBy { get; set; }
        public string? SearchTerm { get; set; }
    }
}
