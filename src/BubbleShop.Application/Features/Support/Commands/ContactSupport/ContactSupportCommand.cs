
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Support.Commands.ContactSupport;

public sealed record ContactSupportCommand(
    string Channel,
    Guid CustomerId,
    string CustomerName,
    Guid BusinessId,
    string Message,
    string? CustomerWhatsApp = null,
    string? CustomerPhone = null,
    string? CustomerEmail = null
) : IRequest<Result<MessageResponse>>;