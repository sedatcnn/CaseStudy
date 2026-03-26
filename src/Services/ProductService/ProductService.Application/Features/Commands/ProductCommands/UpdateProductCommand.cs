using MediatR;
using ProductService.Application.Features.Results.Product;

namespace ProductService.Application.Features.Commands.ProductCommands
{
    /// <summary>
    /// Güncelleme komutu — JWT doğrulaması API katmanında [Authorize] ile sağlanır.
    /// Handler yalnızca iş mantığını içerir; auth bu katmana sızmaz (SRP).
    /// </summary>
    public record UpdateProductCommand(
        Guid ProductId,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        string Category,
        string UpdatedBy) : IRequest<UpdateProductResult>;
}
