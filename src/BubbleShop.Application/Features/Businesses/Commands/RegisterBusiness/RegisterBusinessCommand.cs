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
    string? LegalName = null,
    string? WebhookUrl = null  // Optional webhook for WhatsApp
) : IRequest<Result<BusinessRegistrationResponse>>;


public record BusinessRegistrationResponse(
    Guid BusinessId,
    string BusinessName,
    string WhatsAppNumber,
    Guid ChannelId,
    string ChannelType
);