using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Businesses.Commands.RegisterBusiness;

public sealed record RegisterBusinessCommand(
    string BusinessName,
    string Email,
    string WhatsAppNumber,
    string Password,
    string? PhoneNumber = null,
    string? Address = null,
    string? LegalName = null
) : IRequest<Result<Guid>>;