using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Category> GetAllAsync()
        {
            return _context.Categories
                .Include(x => x.ParentCategory);
        }

        public async Task<Category?> GetByIdAsync(Guid categoryId)
        {
            return await _context.Categories
                .Include(x => x.ParentCategory)
                .FirstOrDefaultAsync(x => x.Id == categoryId);
        }

        public async Task<List<Category>> GetByIdsAsync(List<Guid> categoryIds)
        {
            return await _context.Categories
                .Where(x => categoryIds.Contains(x.Id))
                .Include(x => x.ParentCategory)
                .ToListAsync();
        }

        public async Task<Category?> GetWithSubCategoriesAsync(Guid categoryId)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public IQueryable<Category> GetRootCategoriesAsync()
        {
            return _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.SubCategories)
                        .ThenInclude(ssc => ssc.SubCategories)
                .Where(c => c.ParentCategoryId == null);
        }

        public async Task<bool> ParentCategoryExistsAsync(Guid parentCategoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == parentCategoryId);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}