
//using BubbleShop.Application.Common.Interfaces;
//using BubbleShop.Application.Common.Models;
//using BubbleShop.Domain.Common;
//using BubbleShop.Domain.Enums;
//using Microsoft.Extensions.Logging;

//namespace BubbleShop.Application.Channels;

//public class InstagramChannelAdapter : IChannelAdapter
//{
//    public ChannelType ChannelType => ChannelType.Instagram;

//    private readonly IMessageRouter _messageRouter;
//    private readonly ILogger<InstagramChannelAdapter> _logger;

//    public InstagramChannelAdapter(
//        IMessageRouter messageRouter,
//        ILogger<InstagramChannelAdapter> logger)
//    {
//        _messageRouter = messageRouter;
//        _logger = logger;
//    }

//    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Instagram channel adapter started");
//        // Implement Instagram webhook listener
//        await Task.CompletedTask;
//    }

//    public async Task SendMessageAsync(string userId, string message, CancellationToken cancellationToken = default)
//    {
//        // Implement Instagram DM API
//        _logger.LogInformation("Sending Instagram message to {UserId}: {Message}", userId, message);
//        await Task.CompletedTask;
//    }

//    public async Task SendTypingIndicatorAsync(string userId, CancellationToken cancellationToken = default)
//    {
//        await Task.CompletedTask;
//    }

//    public async Task ProcessIncomingMessageAsync(
//        string from,
//        string message,
//        MessageContext context,
//        CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Processing Instagram message from {From}: {Message}", from, message);
//        var response = await _messageRouter.ProcessIncomingMessageAsync(message, context, cancellationToken);
//        await SendMessageAsync(from, response, cancellationToken);
//    }
//    public async Task SendInteractiveMessageAsync(
//    string userId,
//    InteractiveMessage message,
//    CancellationToken cancellationToken = default)
//    {
//        // TODO: Implement Instagram Interactive Message API
//        _logger.LogInformation(
//            "Sending Instagram interactive message to {UserId}",
//            userId);

//        await Task.CompletedTask;
//    }


//    public async Task<UserProfile?> GetUserProfileAsync(
//        string userId,
//        CancellationToken cancellationToken = default)
//    {
//        // TODO: Implement Instagram User Profile API
//        _logger.LogInformation(
//            "Getting Instagram profile for {UserId}",
//            userId);

//        await Task.CompletedTask;

//        return null;
//    }
//}