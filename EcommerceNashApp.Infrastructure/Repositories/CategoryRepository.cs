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
            var rootCategories = _context.Categories
                .Where(c => c.ParentCategoryId == null);

            LoadSubCategories(rootCategories);

            return rootCategories;
        }

        private void LoadSubCategories(IQueryable<Category> categoriesQuery)
        {
            var categories = categoriesQuery
                .Include(c => c.SubCategories)
                .ToList(); 

            foreach (var category in categories)
            {
                if (category.SubCategories != null && category.SubCategories.Any())
                {
                    // Recursively load subcategories for each category
                    LoadSubCategories(_context.Categories
                        .Where(c => c.ParentCategoryId == category.Id));
                }
            }
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

        public async Task<bool> HasProduct(Guid categoryId)
        {
            return await _context.Categories
                .Where(c => c.Id == categoryId)
                .AnyAsync(c => c.Products.Any());
        }
    }
}