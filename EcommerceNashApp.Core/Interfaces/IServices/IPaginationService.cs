using EcommerceNashApp.Shared.Paginations;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IPaginationService
    {
        Task<PagedList<T>> EF_ToPagedList<T>(IQueryable<T> query, int pageNumber, int pageSize);
    }
}
