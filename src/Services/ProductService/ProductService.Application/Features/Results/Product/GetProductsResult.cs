using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Results.Product
{

    public record GetProductsResult(IReadOnlyList<ProductDto> Products, int TotalCount);
    public record ProductDto(Guid Id, string Name, string Description,
    decimal Price, int Stock, string Category, DateTime CreatedAt);

}
