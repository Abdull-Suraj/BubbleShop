// Application/Features/Businesses/Commands/RegisterBusiness/RegisterBusinessCommandHandler.cs
using BCrypt.Net;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Businesses.Commands.RegisterBusiness;

public sealed class RegisterBusinessCommandHandler : IRequestHandler<RegisterBusinessCommand, Result<BusinessRegistrationResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterBusinessCommandHandler> _logger;

    public RegisterBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IChannelRepository channelRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterBusinessCommandHandler> logger)
    {
        _businessRepository = businessRepository;
        _channelRepository = channelRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BusinessRegistrationResponse>> Handle(RegisterBusinessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registering new business: {BusinessName}", request.BusinessName);

            // Validate email uniqueness
            var existingByEmail = await _businessRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingByEmail is not null)
                return Result<BusinessRegistrationResponse>.Failure($"Email '{request.Email}' is already registered", "ValidationError");

            // Validate WhatsApp uniqueness
            var existingByWhatsApp = await _businessRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);
            if (existingByWhatsApp is not null)
                return Result<BusinessRegistrationResponse>.Failure($"WhatsApp number '{request.WhatsAppNumber}' is already registered", "ValidationError");

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create business entity
            var business = new Business(
                businessName: request.BusinessName,
                email: request.Email,
                whatsAppNumber: request.WhatsAppNumber,
                passwordHash: passwordHash
            );

            // Set optional fields
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                business.UpdatePhoneNumber(request.PhoneNumber);

            if (!string.IsNullOrEmpty(request.Address))
                business.UpdateAddress(request.Address);

            if (!string.IsNullOrEmpty(request.LegalName))
                business.UpdateLegalName(request.LegalName);

            await _businessRepository.AddAsync(business, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ============================================
            // AUTOMATICALLY CREATE WHATSAPP CHANNEL
            // ============================================
            var channel = new Channel(
                businessId: business.Id,
                channelType: ChannelType.WhatsApp,
                webhookUrl: request.WebhookUrl ?? $"https://api.bubbleshop.com/webhooks/whatsapp/{business.Id}",
                apiKey: GenerateApiKey(),
                isActive: true
            );

            await _channelRepository.AddAsync(channel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Business registered successfully: {BusinessId} with WhatsApp Channel: {ChannelId}",
                business.Id, channel.Id);

            return Result<BusinessRegistrationResponse>.Success(new BusinessRegistrationResponse(
                BusinessId: business.Id,
                BusinessName: business.BusinessName,
                WhatsAppNumber: business.WhatsAppNumber,
                ChannelId: channel.Id,
                ChannelType: channel.ChannelType.ToString()
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering business: {BusinessName}", request.BusinessName);
            return Result<BusinessRegistrationResponse>.Failure($"Failed to register business: {ex.Message}");
        }
    }

    private string GenerateApiKey()
    {
        return $"BS-{Guid.NewGuid():N}-{DateTime.Now:yyyyMMdd}";
    }
}