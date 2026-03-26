using MediatR;
using ProductService.Application.Events;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Features.Results.Product;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Handlers.ProductHandlers
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductResult>
    {
        private readonly IProductRepository _repo;
        private readonly IEventPublisher _publisher;
        private readonly ICacheService _cache;

        public UpdateProductCommandHandler(
            IProductRepository repo, IEventPublisher publisher, ICacheService cache)
        {
            _repo = repo;
            _publisher = publisher;
            _cache = cache;
        }

        public async Task<UpdateProductResult> Handle(
            UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repo.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return new UpdateProductResult(false, "Ürün bulunamadı.");

            // Domain metodunu kullan — iş kuralları entity'de kalır
            product.Update(request.Name, request.Description,
                request.Price, request.Stock, request.Category);

            await _repo.UpdateAsync(product, cancellationToken);

            // Cache invalidation — hem liste hem tekil ürün cache'i temizle
            await _cache.RemoveByPrefixAsync("products:", cancellationToken);
            await _cache.RemoveAsync($"product:{request.ProductId}", cancellationToken);

            // Event — Log servisi güncelleme işlemini kaydeder
            var @event = new ProductUpdatedEvent(
                product.Id, product.Name, product.Price,
                product.Category, request.UpdatedBy, DateTime.UtcNow);

            await _publisher.PublishAsync(@event, "product.updated", cancellationToken);

            return new UpdateProductResult(true, "Ürün başarıyla güncellendi.");
        }
    }

}
