using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Shared.Paginations
{
    public class PagedList<T> : List<T>
    {
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Metadata = new PaginationMetadata
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };
            AddRange(items);
        }

        public PaginationMetadata Metadata { get; set; }
    }
}