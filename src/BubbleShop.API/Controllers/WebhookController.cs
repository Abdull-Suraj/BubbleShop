using BubbleShop.Application.Features.Payments.Commands.HandlePaymentWebhook;
using BubbleShop.Application.Features.WhatsApp.Commands.HandleIncomingMessage;
using BubbleShop.Infrastructure.Configuration;
using BubbleShop.Infrastructure.ExternalServices.WhatsApp;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System.Text.Json;

namespace BubbleShop.API.Controllers;

[ApiController]
[Route("api/webhook")]
public sealed class WebhookController(IMediator mediator, IOptions<WhatsAppOptions> whatsAppOptions, IOptions<StripeOptions> stripeOptions) : ControllerBase
{
    [HttpGet("whatsapp")]
    public IActionResult Verify([FromQuery(Name = "hub.mode")] string mode, [FromQuery(Name = "hub.verify_token")] string verifyToken, [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (mode == "subscribe" && verifyToken == whatsAppOptions.Value.VerifyToken)
        {
            return Ok(challenge);
        }

        return Forbid();
    }

    [HttpPost("whatsapp")]
    public async Task<IActionResult> ReceiveWhatsApp([FromBody] WhatsAppWebhookPayload payload, CancellationToken cancellationToken)
    {
        var message = payload.Entry.SelectMany(x => x.Changes).SelectMany(x => x.Value.Messages).FirstOrDefault();
        if (message is null || string.IsNullOrWhiteSpace(message.Text.Body))
        {
            return Ok();
        }

        var result = await mediator.Send(new HandleIncomingMessageCommand(message.From, message.Text.Body), cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("payment")]
    public async Task<IActionResult> ReceivePaymentWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, stripeOptions.Value.WebhookSecret);
        }
        catch
        {
            return BadRequest();
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session?.Metadata.TryGetValue("order_id", out var orderIdRaw) == true && Guid.TryParse(orderIdRaw, out var orderId))
            {
                await mediator.Send(new HandlePaymentWebhookCommand(orderId, session.PaymentIntentId ?? session.Id), cancellationToken);
            }
        }

        return Ok();
    }
}
