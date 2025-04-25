using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid productId)
        {
            return await _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Ratings)
                .Include(x => x.ProductImages)
                .FirstOrDefaultAsync(x => x.Id == productId);
        }

        public IQueryable<Product> GetAllAsync()
        {
            return _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Ratings)
                .Include(x => x.ProductImages);
        }

        public IQueryable<Product> GetByCategoryIdAsync(Guid categoryId)
        {
            return _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Ratings)
                .Include(x => x.ProductImages)
                .Where(x => x.Categories.Any(c => c.Id == categoryId));
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> GetWithImagesAsync(Guid productId)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<Product?> GetWithCategoriesAsync(Guid productId)
        {
            return await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<List<Category>> GetCategoriesByIdsAsync(List<Guid> categoryIds)
        {
            return await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.ProductImages.RemoveRange(product.ProductImages);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}