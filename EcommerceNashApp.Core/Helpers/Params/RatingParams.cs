using EcommerceNashApp.Infrastructure.Helpers.Params.Base;

namespace EcommerceNashApp.Core.Helpers.Params
{
    public class RatingParams : PaginationParams
    {
        public string? OrderBy { get; set; }
        public string? Value { get; set; }
        public string? HasComment { get; set; }
    }
}
