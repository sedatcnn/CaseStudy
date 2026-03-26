using MediatR;
using ProductService.Application.Events;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Features.Results.Product;
using ProductService.Application.Interfaces;
using DomainProduct = ProductService.Domain.Entities.Product;

namespace ProductService.Application.Features.Handlers.ProductHandlers
{
    /// <summary>
    /// SRP: Yalnızca ürün ekleme sorumluluğu.
    /// Ürün eklendikten sonra event fırlatarak diğer servisleri (Log, bildirim) bilgilendirir.
    /// Bu event-driven yaklaşım servislerin birbirinden bağımsız kalmasını sağlar.
    /// </summary>
    public class AddProductCommandHandler : IRequestHandler<AddProductCommand, AddProductResult>
    {
        private readonly IProductRepository _repo;
        private readonly IEventPublisher _publisher;
        private readonly ICacheService _cache;

        public AddProductCommandHandler(
            IProductRepository repo,
            IEventPublisher publisher,
            ICacheService cache)
        {
            _repo = repo;
            _publisher = publisher;
            _cache = cache;
        }

        public async Task<AddProductResult> Handle(
            AddProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Domain factory method ile geçerli ürün oluştur
            var product = DomainProduct.Create(
                request.Name, request.Description,
                request.Price, request.Stock,
                request.Category, request.CreatedBy);

            // 2. Asenkron olarak veritabanına yaz
            await _repo.AddAsync(product, cancellationToken);

            // 3. Cache Invalidation — ürün listesi cache'i temizle
            await _cache.RemoveByPrefixAsync("products:", cancellationToken);

            // 4. Event fırlat — RabbitMQ üzerinden Log servisi ve diğerleri bilgilendirilir
            var @event = new ProductAddedEvent(
                product.Id, product.Name, product.Price,
                product.Category, product.CreatedBy, DateTime.UtcNow);

            await _publisher.PublishAsync(@event, "product.added", cancellationToken);

            return new AddProductResult(true, product.Id, "Ürün başarıyla eklendi.");
        }
    }
}
