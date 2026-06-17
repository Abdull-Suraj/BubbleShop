
using BubbleShop.Application.Features.Customers.Commands.BlockCustomer;
using BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;
using BubbleShop.Application.Features.Customers.Commands.DeleteCustomer;
using BubbleShop.Application.Features.Customers.Commands.UnblockCustomer;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerById;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;
using BubbleShop.Application.Features.Customers.Queries.GetCustomersByBusiness;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all customers for a business
    /// </summary>
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByBusiness(
        Guid businessId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCustomersByBusinessQuery(businessId, pageNumber, pageSize, search), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Get customer by WhatsApp number
    /// </summary>
    [HttpGet("by-whatsapp/{whatsAppNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByWhatsAppNumber(
        string whatsAppNumber,
        [FromQuery] Guid businessId,
        CancellationToken cancellationToken)
    {
        // Fixed: Query now takes 2 arguments (whatsAppNumber, businessId)
        var result = await _mediator.Send(new GetCustomerByWhatsAppNumberQuery(whatsAppNumber, businessId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Create or update a customer
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrUpdate([FromBody] CreateOrUpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    /// <summary>
    /// Block a customer
    /// </summary>
    [HttpPost("{id:guid}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Block(Guid id, [FromBody] BlockCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new BlockCustomerCommand(id, request.Reason), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Unblock a customer
    /// </summary>
    [HttpPost("{id:guid}/unblock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken cancellationToken)
    {
        // Fixed: Added using for UnblockCustomerCommand
        var result = await _mediator.Send(new UnblockCustomerCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }
}

public record BlockCustomerRequest(string? Reason = null);