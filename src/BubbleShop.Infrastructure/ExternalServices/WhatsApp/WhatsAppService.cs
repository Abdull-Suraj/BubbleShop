using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Net.Http.Json;

namespace BubbleShop.Infrastructure.ExternalServices.WhatsApp;

public sealed class WhatsAppService(
    HttpClient httpClient,
    IOptions<WhatsAppOptions> options,
    ILogger<WhatsAppService> logger) : IWhatsAppService
{
    private readonly WhatsAppOptions _options = options.Value;
    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt));

    public async Task SendMessageAsync(string toNumber, string message, CancellationToken cancellationToken = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                to = toNumber,
                text = new { body = message }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.ApiUrl}/{_options.PhoneNumberId}/messages")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);
            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            logger.LogInformation("WhatsApp message sent to {Phone}", toNumber);
        });
    }

    public async Task SendMessageWithButtonsAsync(string toNumber, string message, IReadOnlyCollection<string> buttons, CancellationToken cancellationToken = default)
    {
        await SendMessageAsync(toNumber, $"{message}\nOptions: {string.Join(", ", buttons)}", cancellationToken);
    }
}
