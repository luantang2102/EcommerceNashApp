using EcommerceNashApp.Core.Models.Auth;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface IUserRepository
    {
        IQueryable<AppUser> GetAllAsync();
        Task<AppUser?> GetByIdAsync(Guid userId);
        Task<List<AppUser>> GetUsersInRoleAsync(string role);
        Task<bool> IsInRoleAsync(AppUser user, string role);
        Task<IList<string>> GetRolesAsync(AppUser user);
        Task<AppUser?> FindByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task UpdateAsync(AppUser user);
        Task<bool> CreateAsync(AppUser user, string password);
        Task AddToRoleAsync(AppUser user, string role);
    }
}