using BubbleShop.Application.Features.Payments.Commands.InitiatePayment;
using BubbleShop.Application.Features.Payments.Commands.RefundPayment;
using BubbleShop.Application.Features.Payments.Queries.GetPaymentStatus;
using BubbleShop.Application.Features.Payments.Queries.GetPaymentHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BubbleShop.Application.Features.Payments.Queries.GetCustomerPayments;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
[Produces("application/json")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Initiate a payment for an order
    /// </summary>
    [HttpPost("initiate/{orderId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Initiate(
        Guid orderId,
        [FromQuery] string provider = "flutterwave",
        [FromQuery] string currency = "NGN",
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new InitiatePaymentCommand(orderId, provider, currency), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Get payment status by transaction reference
    /// </summary>
    [HttpGet("status/{transactionReference}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus(string transactionReference, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentStatusQuery(transactionReference), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Get payment history for a business
    /// </summary>
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBusinessPayments(
        Guid businessId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPaymentHistoryQuery(businessId, fromDate, toDate, pageNumber, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get payment history for a customer
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomerPayments(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerPaymentsQuery(customerId), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Refund a payment
    /// </summary>
    [HttpPost("{paymentId:guid}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refund(Guid paymentId, [FromBody] RefundRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefundPaymentCommand(paymentId, request.Amount, request.Reason), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}

public record RefundRequest(decimal Amount, string? Reason = null);