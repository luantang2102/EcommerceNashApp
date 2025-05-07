using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Shared.Paginations.Service.Impl
{
    public class EF_PaginationService : IPaginationService
    {
        public async Task<PagedList<T>> EF_ToPagedList<T>(IQueryable<T> query, int pageNumber, int pageSize)
        {
            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
