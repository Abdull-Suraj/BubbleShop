
//using BubbleShop.Application.Common.Interfaces;
//using BubbleShop.Application.Common.Models;
//using BubbleShop.Domain.Common;
//using BubbleShop.Domain.Enums;
//using BubbleShop.Domain.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System.Text.Json;

//namespace BubbleShop.Application.Channels;

//public class FacebookChannelAdapter : IChannelAdapter
//{
//    public ChannelType ChannelType => ChannelType.Facebook;

//    private readonly HttpClient _httpClient;
//    private readonly IMessageRouter _messageRouter;
//    private readonly IBusinessRepository _businessRepository;
//    private readonly IChannelRepository _channelRepository;
//    private readonly ILogger<FacebookChannelAdapter> _logger;
//    private readonly IConfiguration _configuration;
//    private readonly string _verifyToken;
//    private readonly string _pageAccessToken;

//    public FacebookChannelAdapter(
//        IHttpClientFactory httpClientFactory,
//        IMessageRouter messageRouter,
//        IBusinessRepository businessRepository,
//        ILogger<FacebookChannelAdapter> logger,
//        IChannelRepository channelRepository,
//        IConfiguration configuration)
//    {
//        _httpClient = httpClientFactory.CreateClient();
//        _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v18.0/");

//        _messageRouter = messageRouter;
//        _businessRepository = businessRepository;
//        _logger = logger;
//        _configuration = configuration;
//        _channelRepository = channelRepository;

//        _verifyToken = _configuration["Facebook:VerifyToken"] ?? "your_verify_token";
//        _pageAccessToken = _configuration["Facebook:PageAccessToken"] ?? string.Empty;
//    }

//    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Facebook channel adapter started");
//        // Webhook is handled by controller
//        await Task.CompletedTask;
//    }

//    public async Task SendMessageAsync(string userId, string message, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Sending Facebook message to {UserId}: {Message}", userId, message);

//            var payload = new
//            {
//                recipient = new { id = userId },
//                message = new { text = message }
//            };

//            var content = new StringContent(
//                JsonSerializer.Serialize(payload),
//                System.Text.Encoding.UTF8,
//                "application/json");

//            var response = await _httpClient.PostAsync(
//                $"me/messages?access_token={_pageAccessToken}",
//                content,
//                cancellationToken);

//            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError("Facebook API error: {Error}", responseContent);
//                throw new Exception($"Facebook API error: {responseContent}");
//            }

//            _logger.LogInformation("Facebook message sent successfully to {UserId}", userId);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to send Facebook message to {UserId}", userId);
//            throw;
//        }
//    }

//    public async Task SendTypingIndicatorAsync(string userId, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var payload = new
//            {
//                recipient = new { id = userId },
//                sender_action = "typing_on"
//            };

//            var content = new StringContent(
//                JsonSerializer.Serialize(payload),
//                System.Text.Encoding.UTF8,
//                "application/json");

//            await _httpClient.PostAsync(
//                $"me/messages?access_token={_pageAccessToken}",
//                content,
//                cancellationToken);

//            _logger.LogInformation("Typing indicator sent to {UserId}", userId);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to send typing indicator to {UserId}", userId);
//        }
//    }

//    public async Task ProcessIncomingMessageAsync(
//        string from,
//        string message,
//        MessageContext context,
//        CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Processing Facebook message from {From}: {Message}", from, message);
//        var response = await _messageRouter.ProcessIncomingMessageAsync(message, context, cancellationToken);
//        await SendMessageAsync(from, response, cancellationToken);
//    }

//    /// <summary>
//    /// Process Facebook webhook payload
//    /// </summary>
//    public async Task ProcessWebhookPayloadAsync(JsonDocument payload, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var root = payload.RootElement;

//            // Verify webhook
//            if (root.TryGetProperty("object", out var obj) && obj.GetString() == "page")
//            {
//                if (root.TryGetProperty("entry", out var entries))
//                {
//                    foreach (var entry in entries.EnumerateArray())
//                    {
//                        if (entry.TryGetProperty("messaging", out var messaging))
//                        {
//                            foreach (var msg in messaging.EnumerateArray())
//                            {
//                                await ProcessMessagingEventAsync(msg, cancellationToken);
//                            }
//                        }
//                        else if (entry.TryGetProperty("changes", out var changes))
//                        {
//                            // Handle page changes
//                            await ProcessPageChangeAsync(changes, cancellationToken);
//                        }
//                    }
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error processing Facebook webhook payload");
//            throw;
//        }
//    }

//    private async Task ProcessMessagingEventAsync(JsonElement msg, CancellationToken cancellationToken)
//    {
//        var senderId = msg.GetProperty("sender").GetProperty("id").GetString();
//        var businessId = await GetBusinessIdForPageAsync();

//        if (senderId is null)
//            return;

//        // Handle text message
//        if (msg.TryGetProperty("message", out var message))
//        {
//            if (message.TryGetProperty("text", out var text))
//            {
//                var messageText = text.GetString() ?? string.Empty;

//                // Get user profile
//                var userProfile = await GetUserProfileAsync(senderId, cancellationToken);

//                var context = new MessageContext
//                {
//                    Channel = ChannelType.Facebook,
//                    ChannelUserId = senderId,
//                    BusinessId = businessId.ToString(),
//                    ReceivedAt = DateTime.UtcNow,
//                    Metadata = new Dictionary<string, string>
//                    {
//                        ["CustomerName"] = userProfile?.FirstName ?? "Facebook User",
//                        ["PageId"] = msg.GetProperty("recipient").GetProperty("id").GetString() ?? "",
//                        ["MessageId"] = message.GetProperty("mid").GetString() ?? "",
//                        ["IsEcho"] = message.TryGetProperty("is_echo", out var echo) && echo.GetBoolean() ? "true" : "false"
//                    }
//                };

//                var response = await _messageRouter.ProcessIncomingMessageAsync(messageText, context, cancellationToken);
//                await SendMessageAsync(senderId, response, cancellationToken);
//            }
//        }

//        // Handle postback (button clicks)
//        if (msg.TryGetProperty("postback", out var postback))
//        {
//            var payload = postback.GetProperty("payload").GetString() ?? string.Empty;
//            var title = postback.GetProperty("title").GetString() ?? "Button clicked";

//            var context = new MessageContext
//            {
//                Channel = ChannelType.Facebook,
//                ChannelUserId = senderId,
//                BusinessId = businessId.ToString(),
//                ReceivedAt = DateTime.UtcNow,
//                Metadata = new Dictionary<string, string>
//                {
//                    ["IsPostback"] = "true",
//                    ["PostbackPayload"] = payload,
//                    ["PostbackTitle"] = title
//                }
//            };

//            var response = await _messageRouter.ProcessIncomingMessageAsync(title, context, cancellationToken);
//            await SendMessageAsync(senderId, response, cancellationToken);
//        }
//    }

//    private async Task ProcessPageChangeAsync(JsonElement changes, CancellationToken cancellationToken)
//    {
//        // Handle page changes (e.g., like, follow, unfollow)
//        foreach (var change in changes.EnumerateArray())
//        {
//            var field = change.GetProperty("field").GetString();
//            var value = change.GetProperty("value");

//            _logger.LogInformation("Facebook page change: {Field}", field);

//            if (field == "leadgen")
//            {
//                // Handle lead generation
//                await ProcessLeadGenAsync(value, cancellationToken);
//            }
//        }
//    }

//    private async Task ProcessLeadGenAsync(JsonElement leadGen, CancellationToken cancellationToken)
//    {
//        // Process Facebook lead generation
//        _logger.LogInformation("Facebook lead generated: {LeadId}", leadGen.GetProperty("leadgen_id").GetString());
//        // You can send a welcome message here
//    }

//    /// <summary>
//    /// Get Facebook user profile
//    /// </summary>
//    public async Task<UserProfile> GetUserProfileAsync(
//        string userId,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var response = await _httpClient.GetAsync(
//                $"{userId}?fields=first_name,last_name,profile_pic&access_token={_pageAccessToken}",
//                cancellationToken);

//            if (!response.IsSuccessStatusCode)
//            {
//                return new UserProfile
//                {
//                    Id = userId,
//                    ChannelType = ChannelType.Facebook
//                };
//            }

//            var content = await response.Content.ReadAsStringAsync(cancellationToken);

//            var facebookProfile =
//                JsonSerializer.Deserialize<FacebookUserProfile>(content);

//            return new UserProfile
//            {
//                Id = facebookProfile?.Id ?? userId,
//                ChannelType = ChannelType.Facebook,
//                FirstName = facebookProfile?.FirstName ?? string.Empty,
//                LastName = facebookProfile?.LastName ?? string.Empty,
//                FullName =
//                    $"{facebookProfile?.FirstName} {facebookProfile?.LastName}".Trim(),
//                ProfilePictureUrl = facebookProfile?.ProfilePic
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(
//                ex,
//                "Failed to get Facebook user profile for {UserId}",
//                userId);

//            return new UserProfile
//            {
//                Id = userId,
//                ChannelType = ChannelType.Facebook
//            };
//        }
//    }

//    /// <summary>
//    /// Send a generic template message
//    /// </summary>
//    public async Task SendTemplateMessageAsync(string userId, string title, List<string> buttons, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var payload = new
//            {
//                recipient = new { id = userId },
//                message = new
//                {
//                    attachment = new
//                    {
//                        type = "template",
//                        payload = new
//                        {
//                            template_type = "generic",
//                            elements = new[]
//                            {
//                                new
//                                {
//                                    title = title,
//                                    buttons = buttons.Select((b, i) => new
//                                    {
//                                        type = "postback",
//                                        title = b,
//                                        payload = $"ACTION_{i}"
//                                    }).ToArray()
//                                }
//                            }
//                        }
//                    }
//                }
//            };

//            var content = new StringContent(
//                JsonSerializer.Serialize(payload),
//                System.Text.Encoding.UTF8,
//                "application/json");

//            await _httpClient.PostAsync(
//                $"me/messages?access_token={_pageAccessToken}",
//                content,
//                cancellationToken);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to send template message to {UserId}", userId);
//            throw;
//        }
//    }

//    /// <summary>
//    /// Send quick replies
//    /// </summary>
//    public async Task SendQuickRepliesAsync(string userId, string message, List<string> replies, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var payload = new
//            {
//                recipient = new { id = userId },
//                message = new
//                {
//                    text = message,
//                    quick_replies = replies.Select((r, i) => new
//                    {
//                        content_type = "text",
//                        title = r,
//                        payload = $"QUICK_{i}"
//                    }).ToArray()
//                }
//            };

//            var content = new StringContent(
//                JsonSerializer.Serialize(payload),
//                System.Text.Encoding.UTF8,
//                "application/json");

//            await _httpClient.PostAsync(
//                $"me/messages?access_token={_pageAccessToken}",
//                content,
//                cancellationToken);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to send quick replies to {UserId}", userId);
//            throw;
//        }
//    }
//    public async Task SendInteractiveMessageAsync(
//    string userId,
//    InteractiveMessage message,
//    CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            object facebookMessage;

//            if (message.Buttons.Any())
//            {
//                facebookMessage = new
//                {
//                    text = message.Text,
//                    quick_replies = message.Buttons.Select(button => new
//                    {
//                        content_type = "text",
//                        title = button.Title,
//                        payload = button.Id
//                    })
//                };
//            }
//            else if (message.QuickReplies.Any())
//            {
//                facebookMessage = new
//                {
//                    text = message.Text,
//                    quick_replies = message.QuickReplies.Select(reply => new
//                    {
//                        content_type = "text",
//                        title = reply,
//                        payload = reply
//                    })
//                };
//            }
//            else
//            {
//                facebookMessage = new
//                {
//                    text = message.Text
//                };
//            }


//            var payload = new
//            {
//                recipient = new
//                {
//                    id = userId
//                },
//                message = facebookMessage
//            };


//            var content = new StringContent(
//                JsonSerializer.Serialize(payload),
//                System.Text.Encoding.UTF8,
//                "application/json");


//            var response = await _httpClient.PostAsync(
//                $"me/messages?access_token={_pageAccessToken}",
//                content,
//                cancellationToken);


//            if (!response.IsSuccessStatusCode)
//            {
//                var error = await response.Content.ReadAsStringAsync(cancellationToken);

//                throw new Exception(
//                    $"Facebook interactive message failed: {error}");
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(
//                ex,
//                "Failed to send interactive message to {UserId}",
//                userId);

//            throw;
//        }
//    }
//    private async Task<Guid> GetBusinessIdForPageAsync()
//    {
//        var pageId = _configuration["Facebook:PageId"];

//        if (string.IsNullOrWhiteSpace(pageId))
//            return Guid.Empty;


//        var channel = await _channelRepository.GetByChannelIdAsync(
//            pageId,
//            ChannelType.Facebook);


//        if (channel is not null)
//        {
//            return channel.BusinessId;
//        }


//        var defaultBusinessId = _configuration["Facebook:DefaultBusinessId"];

//        if (Guid.TryParse(defaultBusinessId, out var businessId))
//        {
//            return businessId;
//        }


//        return Guid.Empty;
//    }
//}

///// <summary>
///// Facebook user profile
///// </summary>
//public class FacebookUserProfile
//{
//    public string Id { get; set; } = string.Empty;
//    public string FirstName { get; set; } = string.Empty;
//    public string LastName { get; set; } = string.Empty;
//    public string ProfilePic { get; set; } = string.Empty;
//}