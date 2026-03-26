using MediatR;
using ProductService.Application.Features.Results.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Queries.Product
{
    public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

}
