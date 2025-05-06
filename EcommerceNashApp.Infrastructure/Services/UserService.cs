using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PagedList<UserResponse>> GetUsersAsync(UserParams userParams)
        {
            var usersInRole = await _userRepository.GetUsersInRoleAsync("User");
            var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();

            var query = _userRepository.GetAllAsync()
                .Where(u => userIdsInRole.Contains(u.Id))
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
                var roles = await _userRepository.GetRolesAsync(user);
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
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                var attribute = new Dictionary<string, object>
                {
                    { "UserId", userId.ToString() }
                };
                throw new AppException(ErrorCode.USER_NOT_FOUND, attribute);
            }

            if (!await _userRepository.IsInRoleAsync(user, "User"))
            {
                var attribute = new Dictionary<string, object>
                {
                    { "UserId", userId.ToString() }
                };
                throw new AppException(ErrorCode.USER_NOT_FOUND, attribute);
            }

            var roles = await _userRepository.GetRolesAsync(user);
            return user.MapModelToResponse(roles);
        }
    }
}