using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Shared.Paginations;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Application.Services.Pagination
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
