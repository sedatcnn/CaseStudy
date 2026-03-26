using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    /// <summary>
    /// Redis cache soyutlaması.
    /// OCP: Yeni cache provider eklemek için bu arayüz değişmez.
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
        Task RemoveAsync(string key, CancellationToken ct = default);
        Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
    }

}
