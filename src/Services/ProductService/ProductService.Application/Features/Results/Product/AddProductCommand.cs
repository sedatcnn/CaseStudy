using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Results.Product
{

    public record AddProductResult(bool Success, Guid? ProductId, string? Message);

}
