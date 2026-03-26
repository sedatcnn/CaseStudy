using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Results.Product
{
    public record UpdateProductResult(bool Success, string? Message);

}
