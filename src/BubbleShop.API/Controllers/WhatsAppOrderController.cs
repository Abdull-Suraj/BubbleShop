// API/Controllers/WhatsAppOrderController.cs
using BubbleShop.Application.Features.Orders.Commands.ProcessWhatsAppOrder;
using BubbleShop.Application.Features.Orders.Commands.TrackWhatsAppOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Route("api/whatsapp")]
[Produces("application/json")]
public class WhatsAppOrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WhatsAppOrderController> _logger;

    public WhatsAppOrderController(IMediator mediator, ILogger<WhatsAppOrderController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Customer sends WhatsApp message to order items from a store
    /// </summary>
    [HttpPost("order")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PlaceOrder([FromBody] WhatsAppOrderRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("WhatsApp order request from {Customer} to {Business}",
            request.CustomerWhatsApp, request.BusinessWhatsApp);

        var command = new ProcessWhatsAppOrderCommand(
            CustomerWhatsApp: request.CustomerWhatsApp,
            CustomerName: request.CustomerName,
            BusinessWhatsApp: request.BusinessWhatsApp,
            Message: request.Message
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ErrorCode switch
            {
                "NotFound" => NotFound(new { error = result.Error }),
                "ValidationError" => BadRequest(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return Ok(new
        {
            success = true,
            response = result.Value.ResponseMessage,
            orderId = result.Value.OrderId,
            orderNumber = result.Value.OrderNumber,
            totalAmount = result.Value.TotalAmount,
            productName = result.Value.ProductName,
            quantity = result.Value.Quantity
        });
    }

    [HttpPost("track")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrackOrder([FromBody] WhatsAppTrackRequest request, CancellationToken cancellationToken)
    {
        // Find order by number
        var command = new TrackWhatsAppOrderCommand(
            OrderNumber: request.OrderNumber,
            CustomerWhatsApp: request.CustomerWhatsApp
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new
        {
            success = true,
            orderStatus = result.Value
        });
    }
}

public class WhatsAppOrderRequest
{
    public string CustomerWhatsApp { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string BusinessWhatsApp { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class WhatsAppTrackRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerWhatsApp { get; set; } = string.Empty;
}