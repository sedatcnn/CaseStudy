using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    /// <summary>
    /// Ürün repository soyutlaması.
    /// DIP: Application katmanı SQL/EF detaylarını bilmez.
    /// ISP: Yalnızca ürüne özgü operasyonlar burada.
    /// </summary>
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken ct = default);
        Task AddAsync(Product product, CancellationToken ct = default);
        Task UpdateAsync(Product product, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}
