using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Shared.Paginations.Service
{
    public interface IPaginationService
    {
        Task<PagedList<T>> EF_ToPagedList<T>(IQueryable<T> query, int pageNumber, int pageSize);
    }
}
