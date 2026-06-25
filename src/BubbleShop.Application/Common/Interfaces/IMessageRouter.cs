// Application/Common/Interfaces/IMessageRouter.cs
using BubbleShop.Domain.Common;

namespace BubbleShop.Application.Common.Interfaces;

public interface IMessageRouter
{
    Task<string> ProcessIncomingMessageAsync(string message, MessageContext context, CancellationToken cancellationToken = default);
}