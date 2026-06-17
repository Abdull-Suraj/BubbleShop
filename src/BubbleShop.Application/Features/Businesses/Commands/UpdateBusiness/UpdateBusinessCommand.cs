using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Businesses.Commands.UpdateBusiness;

public sealed record UpdateBusinessCommand(
    Guid BusinessId,
    string BusinessName,
    string Email,
    string WhatsAppNumber,
    string? PhoneNumber = null,
    string? Address = null,
    string? LegalName = null
) : IRequest<Result>;