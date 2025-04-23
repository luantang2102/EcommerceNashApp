using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Models.Auth;

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

        public static UserResponse MapModelToResponse(this AppUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                ImageUrl = user.ImageUrl,
                PublicId = user.PublicId,
                Email = user.Email,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate,
                UserProfiles = user.UserProfiles.Select(x => x.MapModelToResponse()).ToList()
            };
        }
    }
}
