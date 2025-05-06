using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class UserExtensions
    {
        public static IQueryable<AppUser> Sort(this IQueryable<AppUser> query, string? orderBy)
        {
            query = orderBy switch
            {
                "dateCreatedDesc" => query.OrderByDescending(x => x.CreatedDate),
                _ => query.OrderBy(x => x.CreatedDate),
            };
            return query;
        }

        public static IQueryable<AppUser> Search(this IQueryable<AppUser> query, string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return query;
            var lowerCaseTerm = searchTerm.Trim().ToLower();
            return query.Where(x => x.UserName!.ToLower().Contains(lowerCaseTerm) || x.Email!.ToLower().Contains(lowerCaseTerm));
        }

        public static UserResponse MapModelToResponse(this AppUser user, IList<string> roles = null!)
        {
            roles ??= [];
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                ImageUrl = user.ImageUrl,
                PublicId = user.PublicId,
                Email = user.Email,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate,
                Roles = roles.ToList(),
                UserProfiles = user.UserProfiles.Select(x => x.MapModelToResponse()).ToList()
            };
        }
    }
}
