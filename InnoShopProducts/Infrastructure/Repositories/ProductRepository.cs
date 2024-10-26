using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal sealed class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Add(Product product)
        {
            _context.Products.Add(product);
        }
        public void Update(Product product)
        {
            _context.Products.Update(product);
        }
        public void Remove(Product product)
        {
            _context.Products.Remove(product);
        }
        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.SingleOrDefaultAsync(p => p.Id == id);
        }
        public async Task<List<Product>?> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }
        public async Task<List<Product>?> SearchProductsAsync(
            string? name, 
            decimal? minPrice, 
            decimal? maxPrice, 
            int? minQuantity, 
            int? maxQuantity, 
            CancellationToken cancellationToken)
        {
            return await _context.Products
                .Where(p => (name == null || p.Name.Contains(name)) &&
                            (minPrice == null || p.Price >= minPrice) &&
                            (maxPrice == null || maxPrice == 0 || p.Price <= maxPrice) &&
                            (minQuantity == null || p.Quantity >= minQuantity) &&
                            (maxQuantity == null || maxQuantity == 0 || p.Quantity <= maxQuantity))
                .ToListAsync(cancellationToken);
        }
    }
}
