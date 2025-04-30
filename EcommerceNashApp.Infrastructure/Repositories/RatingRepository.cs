using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly AppDbContext _context;

        public RatingRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Rating> GetAllAsync()
        {
            return _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product);
        }

        public async Task<Rating?> GetByIdAsync(Guid ratingId)
        {
            return await _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == ratingId);
        }

        public IQueryable<Rating> GetByProductIdAsync(Guid productId)
        {
            return _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product)
                .Where(x => x.ProductId == productId);
        }

        public async Task<Rating?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            return await _context.Ratings
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        }

        public async Task<AppUser?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<Rating> CreateAsync(Rating rating)
        {
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            return rating;
        }

        public async Task UpdateAsync(Rating rating)
        {
            _context.Ratings.Update(rating);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Rating rating)
        {
            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
        }
    }
}