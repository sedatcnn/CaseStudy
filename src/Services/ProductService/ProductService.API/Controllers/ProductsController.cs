using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Features.Queries.Product;
using System.Security.Claims;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductsController(IMediator mediator) => _mediator = mediator;

    /// <summary> Tüm ürünleri listeler (Redis Cache kullanılır) </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetProductsQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary> ID'ye göre tek bir ürün getirir </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary> Yeni ürün ekler (Sadece Admin ve Manager) </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddProduct([FromBody] AddProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var command = new AddProductCommand(request.Name, request.Description, request.Price, request.Stock, request.Category, userId);

        var result = await _mediator.Send(command);
        return result.Success ? CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, result) : BadRequest(result);
    }

    /// <summary> Ürün bilgilerini günceller </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Stock, request.Category, userId);

        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary> Ürünü siler (Sadece Admin) </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        // İleride Soft Delete eklenecek
        return NoContent();
    }
}

public record AddProductRequest(string Name, string Description, decimal Price, int Stock, string Category);
public record UpdateProductRequest(string Name, string Description, decimal Price, int Stock, string Category);