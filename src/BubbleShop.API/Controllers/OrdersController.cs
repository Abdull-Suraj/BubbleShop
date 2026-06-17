
using BubbleShop.Application.Features.Orders.Commands.CancelOrder;
using BubbleShop.Application.Features.Orders.Commands.ConfirmOrder;
using BubbleShop.Application.Features.Orders.Commands.CreateOrder;
using BubbleShop.Application.Features.Orders.Commands.UpdateOrderStatus;
using BubbleShop.Application.Features.Orders.Queries.GetAllOrders;
using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
using BubbleShop.Application.Features.Orders.Queries.GetOrdersByBusiness;
using BubbleShop.Application.Features.Orders.Queries.GetOrdersByCustomer;
using BubbleShop.Application.Features.Orders.Queries.TrackOrderByNumber;
using BubbleShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all orders with pagination and filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? businessId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllOrdersQuery(pageNumber, pageSize, businessId, status, fromDate, toDate), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrdersByCustomerQuery(customerId), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get orders by business ID
    /// </summary>
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByBusiness(
        Guid businessId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetOrdersByBusinessQuery(businessId, pageNumber, pageSize, status), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { orderId = result.Value });
    }

    /// <summary>
    /// Confirm an order
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmOrderCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelOrderCommand(id, request.Reason), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        // Fixed: Convert string to OrderStatus enum
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            return BadRequest(new { error = "Invalid order status" });

        var result = await _mediator.Send(new UpdateOrderStatusCommand(id, status), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Track order by order number (no authentication required)
    /// </summary>
    [HttpGet("track/{orderNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track(string orderNumber, [FromQuery] string? email = null, CancellationToken cancellationToken = default)
    {
        // Fixed: TrackOrderByNumberQuery now exists
        var result = await _mediator.Send(new TrackOrderByNumberQuery(orderNumber, email), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
}

public record CancelOrderRequest(string? Reason = null);
public record UpdateOrderStatusRequest(string Status);