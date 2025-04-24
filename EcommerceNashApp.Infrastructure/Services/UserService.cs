using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Shared.Paginations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<PagedList<UserResponse>> GetUsersAsync(UserParams userParams)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("User");

            // Filtering User role
            var query = _userManager.Users
                .Where(u => usersInRole.Select(r => r.Id).Contains(u.Id))
                .Include(x => x.UserProfiles)
                .Search(userParams.SearchTerm)
                .Sort(userParams.OrderBy);

            var pagedList = await PagedList<AppUser>.ToPagedList(
                query,
                userParams.PageNumber,
                userParams.PageSize
            );

            var usersWithRoles = new List<UserResponse>();
            foreach (var user in pagedList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(user.MapModelToResponse(roles));
            }

            return new PagedList<UserResponse>(
                usersWithRoles,
                pagedList.Metadata.TotalCount,
                userParams.PageNumber,
                userParams.PageSize
            );
        }

        public async Task<UserResponse> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(x => x.UserProfiles)
                .FirstOrDefaultAsync(x => x.Id == userId); 

            if (user == null)
            {
                var attribute = new Dictionary<string, object>
                {
                    { "UserId", userId.ToString() }
                };
                throw new AppException(ErrorCode.USER_NOT_FOUND, attribute);
            }

            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                var attribute = new Dictionary<string, object>
                {
                    { "UserId", userId.ToString() }
                };
                throw new AppException(ErrorCode.USER_NOT_FOUND, attribute);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return user.MapModelToResponse(roles);
        }
    }
}