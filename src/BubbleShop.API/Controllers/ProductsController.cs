using BubbleShop.Application.Features.Products.Commands.CreateProduct;
using BubbleShop.Application.Features.Products.Commands.UpdateProduct;
using BubbleShop.Application.Features.Products.Commands.UpdateStock;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using BubbleShop.Application.Features.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok((await mediator.Send(new GetAllProductsQuery(), cancellationToken)).Value);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { ProductId = id }, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { ProductId = id }, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
