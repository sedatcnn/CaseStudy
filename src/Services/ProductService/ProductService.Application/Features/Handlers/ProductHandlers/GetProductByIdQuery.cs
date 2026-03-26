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

    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _repo;
        private readonly ICacheService _cache;

        public GetProductByIdQueryHandler(IProductRepository repo, ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"product:{request.Id}";
            var cached = await _cache.GetAsync<ProductDto>(cacheKey, cancellationToken);
            if (cached is not null) return cached;

            var product = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (product is null) return null;

            var dto = new ProductDto(product.Id, product.Name, product.Description,
                product.Price, product.Stock, product.Category, product.CreatedAt);

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15), cancellationToken);
            return dto;
        }
    }
}
