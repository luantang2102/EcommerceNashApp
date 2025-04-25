using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface IRatingRepository
    {
        IQueryable<Rating> GetAllAsync();
        Task<Rating?> GetByIdAsync(Guid ratingId);
        IQueryable<Rating> GetByProductIdAsync(Guid productId);
        Task<Rating?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<AppUser?> GetUserByIdAsync(Guid userId);
        Task<Rating> CreateAsync(Rating rating);
        Task UpdateAsync(Rating rating);
        Task DeleteAsync(Rating rating);
    }
}