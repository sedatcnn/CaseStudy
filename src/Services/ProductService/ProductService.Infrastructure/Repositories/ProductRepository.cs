using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core tabanlı product repository.
    /// LSP: IProductRepository'nin tam anlamıyla yerine geçer.
    /// DIP: Application katmanı bu sınıfı bilmez, sadece interface'i bilir.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _db;

        public ProductRepository(ProductDbContext db) => _db = db;

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

        public async Task<IReadOnlyList<Product>> GetAllAsync(
            int page, int pageSize, CancellationToken ct = default)
            => await _db.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
            string category, CancellationToken ct = default)
            => await _db.Products
                .AsNoTracking()
                .Where(p => p.IsActive && p.Category == category)
                .ToListAsync(ct);

        public async Task AddAsync(Product product, CancellationToken ct = default)
        {
            await _db.Products.AddAsync(product, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => await _db.Products.AnyAsync(p => p.Id == id && p.IsActive, ct);
    }
}
