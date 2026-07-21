using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BubbleShop.Application.Channels;

public sealed class WhatsAppChannelAdapter : IChannelAdapter
{
    private readonly ILogger<WhatsAppChannelAdapter> _logger;
    private readonly HttpClient _httpClient;
    private readonly WhatsAppSettings _settings;
    public WhatsAppChannelAdapter(
      HttpClient httpClient,
      IOptions<WhatsAppSettings> options,
      ILogger<WhatsAppChannelAdapter> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public ChannelType ChannelType => ChannelType.WhatsApp;

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp adapter started.");

        // TODO:
        // Subscribe to webhook or start polling.

        await Task.CompletedTask;
    }

    public async Task SendMessageAsync(
        string userId,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending WhatsApp message to {UserId}: {Message}",
            userId,
            message);

        // TODO:
        // Call WhatsApp Cloud API

        await Task.CompletedTask;
    }

    public async Task SendTypingIndicatorAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // WhatsApp Cloud API currently has no typing indicator.

        _logger.LogDebug("Typing indicator requested for {UserId}", userId);

        await Task.CompletedTask;
    }

    public async Task ProcessIncomingMessageAsync(
        string from,
        string message,
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received WhatsApp message from {UserId}: {Message}",
            from,
            message);

        // TODO:
        // Forward to AI Intent Engine

        await Task.CompletedTask;
    }

    public async Task SendInteractiveMessageAsync(
        string userId,
        InteractiveMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending interactive message to {UserId}",
            userId);

        _logger.LogInformation("Text: {Text}", message.Text);

        foreach (var button in message.Buttons)
        {
            _logger.LogInformation(
                "Button: {Title} ({Id})",
                button.Title,
                button.Id);
        }

        // TODO:
        // Convert InteractiveMessage
        // into WhatsApp Interactive Button/List message
        // and send through Meta Cloud API.

        await Task.CompletedTask;
    }

    public async Task<UserProfile> GetUserProfileAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Meta Cloud API does not expose customer profile
        // information directly.

        return await Task.FromResult(new UserProfile
        {
            Id = userId,
            FirstName = string.Empty,
            LastName = string.Empty,
            FullName = userId,
            Phone = userId,
            Email = null,
            Language = "en",
            ProfilePictureUrl = null
        });
    }
}