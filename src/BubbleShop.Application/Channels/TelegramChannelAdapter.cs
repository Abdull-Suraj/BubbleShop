
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Application.Channels;

public class TelegramChannelAdapter : BackgroundService, IChannelAdapter
{
    public ChannelType ChannelType => ChannelType.Telegram;

    private readonly ITelegramBotClient _botClient;
    private readonly IMessageRouter _messageRouter;
    private readonly IBusinessRepository _businessRepository;
    private readonly ILogger<TelegramChannelAdapter> _logger;
    private readonly IConfiguration _configuration;

    public TelegramChannelAdapter(
        IMessageRouter messageRouter,
        IBusinessRepository businessRepository,
        ILogger<TelegramChannelAdapter> logger,
        IConfiguration configuration)
    {
        _messageRouter = messageRouter;
        _businessRepository = businessRepository;
        _logger = logger;
        _configuration = configuration;

        var botToken = _configuration["Telegram:BotToken"];
        if (string.IsNullOrEmpty(botToken))
        {
            _logger.LogWarning("Telegram bot token not configured");
        }
        else
        {
            _botClient = new TelegramBotClient(botToken);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_botClient == null)
        {
            _logger.LogError("Telegram bot client not initialized");
            return;
        }

        _logger.LogInformation("Telegram channel adapter started");

        var receiverOptions = new Telegram.Bot.Polling.ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.InlineQuery
            }
        };

        _botClient.StartReceiving(
            handleUpdateAsync: OnMessageReceived,
            handleErrorAsync: OnError,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);

        await Task.Delay(-1, stoppingToken);
    }

    private async Task OnMessageReceived(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // Handle different update types
            if (update.Message?.Text is not null)
            {
                await HandleTextMessage(client, update.Message, cancellationToken);
            }
            else if (update.CallbackQuery is not null)
            {
                await HandleCallbackQuery(client, update.CallbackQuery, cancellationToken);
            }
            else if (update.Message is not null)
            {
                await HandleNonTextMessage(client, update.Message, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Telegram message");
        }
    }

    private async Task HandleTextMessage(ITelegramBotClient client, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id.ToString();
        var messageText = message.Text ?? string.Empty;

        // Get business ID from bot token (mapped in database)
        var businessId = await GetBusinessIdForTelegramBotAsync();

        var context = new MessageContext
        {
            Channel = ChannelType.Telegram,
            ChannelUserId = chatId,
            BusinessId = businessId.ToString(),
            ReceivedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["CustomerName"] = message.Chat.Username ?? message.Chat.FirstName ?? "Telegram User",
                ["FirstName"] = message.Chat.FirstName ?? "",
                ["LastName"] = message.Chat.LastName ?? "",
                ["ChatId"] = chatId,
                ["MessageId"] = message.MessageId.ToString()
            }
        };

        // Show typing indicator
        await client.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken: cancellationToken);

        // Process message through router
        var response = await _messageRouter.ProcessIncomingMessageAsync(messageText, context, cancellationToken);

        // Send response with inline keyboard suggestions
        var suggestions = GetSuggestionsFromContext(context);
        if (suggestions.Any())
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                suggestions.Select(s => new[] { InlineKeyboardButton.WithCallbackData(s, $"action_{s.Replace(" ", "_")}") })
            );

            await client.SendTextMessageAsync(
                chatId: chatId,
                text: response,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        else
        {
            await client.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
        }
    }

    private async Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message?.Chat.Id.ToString() ?? string.Empty;
        var data = callbackQuery.Data ?? string.Empty;

        // Handle button clicks
        var action = data.Replace("action_", "").Replace("_", " ");

        // Acknowledge callback
        await client.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);

        // Process as a message
        var message = $"I want to {action}";
        await client.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken: cancellationToken);

        var context = new MessageContext
        {
            Channel = ChannelType.Telegram,
            ChannelUserId = chatId,
            BusinessId = (await GetBusinessIdForTelegramBotAsync()).ToString(),
            ReceivedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["IsCallback"] = "true",
                ["CallbackData"] = data
            }
        };

        var response = await _messageRouter.ProcessIncomingMessageAsync(message, context, cancellationToken);
        await client.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
    }

    private async Task HandleNonTextMessage(ITelegramBotClient client, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id.ToString();
        var response = "I can only process text messages at the moment. Please type your message. 📝";
        await client.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
    }

    private Task OnError(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram bot error");
        return Task.CompletedTask;
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        // Already started in ExecuteAsync
        await Task.CompletedTask;
    }

    public async Task SendMessageAsync(string userId, string message, CancellationToken cancellationToken = default)
    {
        if (_botClient == null)
        {
            _logger.LogError("Telegram bot not initialized");
            return;
        }

        try
        {
            var chatId = new ChatId(long.Parse(userId));
            await _botClient.SendTextMessageAsync(chatId, message, cancellationToken: cancellationToken);
            _logger.LogInformation("Telegram message sent to {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message to {UserId}", userId);
            throw;
        }
    }

    public async Task SendTypingIndicatorAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_botClient == null) return;

        try
        {
            var chatId = new ChatId(long.Parse(userId));
            await _botClient.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send typing indicator to {UserId}", userId);
        }
    }

    public async Task ProcessIncomingMessageAsync(
        string from,
        string message,
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing Telegram message from {From}: {Message}", from, message);
        var response = await _messageRouter.ProcessIncomingMessageAsync(message, context, cancellationToken);
        await SendMessageAsync(from, response, cancellationToken);
    }

    private async Task<Guid> GetBusinessIdForTelegramBotAsync()
    {
        // Map Telegram bot token to business ID from database
        // This should be stored in a configuration or database
        var businessId = _configuration["Telegram:DefaultBusinessId"];
        if (!string.IsNullOrEmpty(businessId) && Guid.TryParse(businessId, out var id))
            return id;

        // Fallback: find business by bot token
        var businesses = await _businessRepository.GetAllAsync();
        var business = businesses.FirstOrDefault(b =>
            b.Metadata != null &&
            b.Metadata.ContainsKey("TelegramBotToken") &&
            b.Metadata["TelegramBotToken"] == _configuration["Telegram:BotToken"]);

        return business?.Id ?? Guid.Empty;
    }

    private List<string> GetSuggestionsFromContext(MessageContext context)
    {
        var suggestions = new List<string>();

        if (context.Metadata.TryGetValue("SuggestedReplies", out var replies))
        {
            suggestions = replies.Split('|').ToList();
        }
        else
        {
            // Default suggestions
            suggestions = new List<string>
            {
                "Buy Products",
                "Check Prices",
                "Track Order",
                "Get Help"
            };
        }

        return suggestions;
    }
}