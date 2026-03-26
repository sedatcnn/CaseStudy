using MediatR;
using ProductService.Application.Features.Queries.Product;
using ProductService.Application.Features.Results.Product;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Handlers.ProductHandlers
{
    /// <summary>
    /// Cache-first sorgu stratejisi:
    /// 1. Redis'te varsa → direkt döner (veritabanına gitmez)
    /// 2. Yoksa → veritabanından çeker, cache'e yazar
    /// Cache Invalidation: AddProduct/UpdateProduct command'larında tetiklenir.
    /// </summary>
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResult>
    {
        private readonly IProductRepository _repo;
        private readonly ICacheService _cache;
        // Cache key'i prefix + sayfalama parametreleriyle oluştur
        private static string CacheKey(int page, int size) => $"products:list:{page}:{size}";
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

        public GetProductsQueryHandler(IProductRepository repo, ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<GetProductsResult> Handle(
            GetProductsQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKey(request.Page, request.PageSize);

            // 1. Cache'den oku
            var cached = await _cache.GetAsync<GetProductsResult>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            // 2. Veritabanından getir
            var products = await _repo.GetAllAsync(request.Page, request.PageSize, cancellationToken);

            var result = new GetProductsResult(
                products.Select(p => new ProductDto(
                    p.Id, p.Name, p.Description,
                    p.Price, p.Stock, p.Category, p.CreatedAt)).ToList(),
                products.Count);

            // 3. Redis'e yaz
            await _cache.SetAsync(cacheKey, result, CacheExpiry, cancellationToken);

            return result;
        }
    }
}
