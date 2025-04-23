using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        private async Task<IQueryable<AppUser>> GetUsersByRolesAsync(List<UserRole> roles)
        {
            if (roles == null || roles.Count == 0)
            {
                return Enumerable.Empty<AppUser>().AsQueryable();
            }

            var usersInRoles = new List<AppUser>();

            foreach (var role in roles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role.ToString());
                usersInRoles.AddRange(users);
            }

            return usersInRoles.Distinct().AsQueryable();
        }

        public async Task<PagedList<UserResponse>> GetUsersAsync(UserParams userParams)
        {
            var usersQuery = await GetUsersByRolesAsync([UserRole.User]);
            var query = usersQuery
                .Include(x => x.UserProfiles)
                .Search(userParams.SearchTerm)
                .Sort(userParams.OrderBy)
                .AsQueryable();

            var projectedQuery = query.Select(x => x.MapModelToResponse(_userManager.GetRolesAsync(x).Result));

            return await PagedList<UserResponse>.ToPagedList(
                projectedQuery,
                userParams.PageNumber,
                userParams.PageSize
            );
        }

        public async Task<UserResponse> GetUserByIdAsync(Guid userId)
        {
            var usersQuery = await GetUsersByRolesAsync([UserRole.User]);
            var user = await usersQuery
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

            return user.MapModelToResponse(_userManager.GetRolesAsync(user).Result);
        }
    }
}
