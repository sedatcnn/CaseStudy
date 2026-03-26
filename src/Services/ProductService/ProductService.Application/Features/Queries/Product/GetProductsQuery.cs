using MediatR;
using ProductService.Application.Features.Results.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Queries.Product
{
    /// <summary>
    /// Ürün listeleme sorgusu — CQRS'te Query side.
    /// IRequest kullanımı: Komutlardan tamamen ayrı; veritabanını değiştirmez.
    /// </summary>
    public record GetProductsQuery(int Page = 1, int PageSize = 20) : IRequest<GetProductsResult>;

}
