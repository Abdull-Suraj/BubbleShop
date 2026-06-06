using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Polly;
using Stripe;
using Stripe.Checkout;

namespace BubbleShop.Infrastructure.ExternalServices.Payment;

public sealed class StripePaymentService(IOptions<StripeOptions> options) : IPaymentService
{
    private readonly StripeOptions _options = options.Value;

    public async Task<string> CreatePaymentLinkAsync(Guid orderId, decimal amount, string description, CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _options.SecretKey;
        var retryPolicy = Policy.Handle<StripeException>().WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * attempt));

        return await retryPolicy.ExecuteAsync(async () =>
        {
            var service = new SessionService();
            var session = await service.CreateAsync(new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = _options.SuccessUrl,
                CancelUrl = _options.CancelUrl,
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmountDecimal = amount * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order {orderId}",
                                Description = description
                            }
                        }
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = orderId.ToString()
                }
            }, cancellationToken: cancellationToken);

            return session.Url ?? string.Empty;
        });
    }
}
