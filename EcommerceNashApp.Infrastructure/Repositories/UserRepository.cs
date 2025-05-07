using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public UserRepository(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IQueryable<AppUser> GetAllAsync()
    {
        return _context.Users
            .Include(x => x.UserProfiles);
    }

    public async Task<AppUser?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(x => x.UserProfiles)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<List<AppUser>> GetUsersInRoleAsync(string role)
    {
        return (await _userManager.GetUsersInRoleAsync(role)).ToList();
    }

    public async Task<bool> IsInRoleAsync(AppUser user, string role)
    {
        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<IList<string>> GetRolesAsync(AppUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<AppUser?> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> CheckPasswordAsync(AppUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task UpdateAsync(AppUser user)
    {
        await _userManager.UpdateAsync(user);
    }

    public async Task<bool> CreateAsync(AppUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded; // Return whether the operation succeeded
    }

    public async Task AddToRoleAsync(AppUser user, string role)
    {
        await _userManager.AddToRoleAsync(user, role);
    }

    // New method to find user by refresh token
    public async Task<AppUser?> FindByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }
}