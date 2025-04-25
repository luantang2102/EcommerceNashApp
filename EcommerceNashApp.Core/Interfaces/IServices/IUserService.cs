using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IUserService
    {
        Task<UserResponse> GetUserByIdAsync(Guid userId);
        Task<PagedList<UserResponse>> GetUsersAsync(UserParams userParams);
    }
}
