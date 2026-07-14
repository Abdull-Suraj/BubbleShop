// Application/Common/Interfaces/IMessageRouter.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.AppServices;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Common.Interfaces;

public interface IMessageRouter
{
    Task<MessageResponse> ProcessIncomingMessageAsync(string message, MessageContext context, CancellationToken cancellationToken = default);
    
}