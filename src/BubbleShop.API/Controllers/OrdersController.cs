using BubbleShop.Application.Features.Orders.Commands.UpdateOrderStatus;
using BubbleShop.Application.Features.Orders.Queries.GetAllOrders;
using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
using BubbleShop.Application.Features.Orders.Queries.GetOrdersByCustomer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok((await mediator.Send(new GetAllOrdersQuery(), cancellationToken)).Value);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
        => Ok((await mediator.Send(new GetOrdersByCustomerQuery(customerId), cancellationToken)).Value);

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { OrderId = id }, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
