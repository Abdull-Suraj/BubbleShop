using BubbleShop.Application.Features.Payments.Commands.HandlePaymentWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BubbleShop.API.Controllers;

[ApiController]
[Route("api/webhooks")]
[Produces("application/json")]
public sealed class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IMediator mediator, ILogger<WebhooksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Flutterwave payment webhook
    /// </summary>
    [HttpPost("flutterwave")]
    public async Task<IActionResult> FlutterwaveWebhook(CancellationToken cancellationToken)
    {
        try
        {
            // Read the raw payload from the request body
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);

            var signature = Request.Headers["verif-hash"].FirstOrDefault() ?? string.Empty;

            // Parse the payload to extract OrderId and TransactionId
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            // Extract transaction ID
            var transactionId = data.GetProperty("id").GetString() ?? string.Empty;

            // Extract order ID from metadata or tx_ref
            var orderId = ExtractOrderIdFromPayload(data);

            if (orderId == Guid.Empty)
            {
                _logger.LogWarning("Could not extract OrderId from Flutterwave webhook payload");
                return Ok(new { status = "received" });
            }

            var command = new HandlePaymentWebhookCommand(
                OrderId: orderId,
                TransactionId: transactionId,
                GatewayResponse: payload,
                Provider: "Flutterwave"
            );

            await _mediator.Send(command, cancellationToken);
            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutterwave webhook error: {Error}", ex.Message);
            // Always return 200 to Flutterwave to prevent retries
            return Ok(new { status = "received" });
        }
    }

    private Guid ExtractOrderIdFromPayload(JsonElement data)
    {
        // Try to get order_id from metadata
        if (data.TryGetProperty("meta", out var meta) &&
            meta.TryGetProperty("order_id", out var orderIdProp))
        {
            if (Guid.TryParse(orderIdProp.GetString(), out var orderId))
                return orderId;
        }

        // Try to extract from tx_ref
        if (data.TryGetProperty("tx_ref", out var txRef))
        {
            var txRefValue = txRef.GetString() ?? string.Empty;
            var parts = txRefValue.Split('-');
            foreach (var part in parts)
            {
                if (Guid.TryParse(part, out var guid))
                    return guid;
            }
        }

        return Guid.Empty;
    }

}