using MediatR;
using ProductService.Application.Features.Results.Product;

namespace ProductService.Application.Features.Commands.ProductCommands
{
    /// <summary>
    /// Ürün ekleme komutu.
    /// Record kullanımı: immutable command — CQRS'in "değişmez istek" ilkesine uygun.
    /// </summary>
    public record AddProductCommand(
        string Name,
        string Description,
        decimal Price,
        int Stock,
        string Category,
        string CreatedBy) : IRequest<AddProductResult>;
}
