using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public interface IProductRepository
    {
        void Add(Product product);
        void Update(Product product);
        void Remove(Product product);
        Task<Product?> GetByIdAsync(Guid id);
        Task<List<Product>?> GetAllAsync();
        Task<List<Product>?> SearchProductsAsync(
            string? name, 
            decimal? minPrice, 
            decimal? maxPrice, 
            int? minQuantity, 
            int? maxQuantity, 
            CancellationToken cancellationToken);
    }
}
