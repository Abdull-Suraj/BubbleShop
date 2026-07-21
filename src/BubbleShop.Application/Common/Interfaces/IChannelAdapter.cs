// Application/Common/Interfaces/IChannelAdapter.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Application.Common.Interfaces;

public interface IChannelAdapter
{
    ChannelType ChannelType { get; }
    Task StartListeningAsync(CancellationToken cancellationToken = default);
    Task SendMessageAsync(string userId, string message, CancellationToken cancellationToken = default);
    Task SendTypingIndicatorAsync(string userId, CancellationToken cancellationToken = default);
    Task ProcessIncomingMessageAsync(string from, string message, MessageContext context, CancellationToken cancellationToken = default);
    Task SendInteractiveMessageAsync(string userId, InteractiveMessage message, CancellationToken cancellationToken = default);
    Task<UserProfile> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
}