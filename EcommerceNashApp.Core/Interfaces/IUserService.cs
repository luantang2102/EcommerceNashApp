using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetUserByIdAsync(Guid userId);
        Task<PagedList<UserResponse>> GetUsersAsync(UserParams userParams);
    }
}
