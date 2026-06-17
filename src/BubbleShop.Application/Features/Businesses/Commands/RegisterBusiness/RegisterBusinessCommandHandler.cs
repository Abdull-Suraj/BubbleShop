using BCrypt.Net;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
//using BCrypt;

namespace BubbleShop.Application.Features.Businesses.Commands.RegisterBusiness;

public sealed class RegisterBusinessCommandHandler : IRequestHandler<RegisterBusinessCommand, Result<Guid>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterBusinessCommandHandler> _logger;

    public RegisterBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterBusinessCommandHandler> logger)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterBusinessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registering new business: {BusinessName}", request.BusinessName);

            // Validate email uniqueness
            var existingByEmail = await _businessRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingByEmail is not null)
                return Result<Guid>.Failure($"Email '{request.Email}' is already registered", "ValidationError");

            // Validate WhatsApp uniqueness
            var existingByWhatsApp = await _businessRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);
            if (existingByWhatsApp is not null)
                return Result<Guid>.Failure($"WhatsApp number '{request.WhatsAppNumber}' is already registered", "ValidationError");

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

            _logger.LogInformation("Business registered successfully: {BusinessId} - {BusinessName}", business.Id, business.BusinessName);

            return Result<Guid>.Success(business.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering business: {BusinessName}", request.BusinessName);
            return Result<Guid>.Failure($"Failed to register business: {ex.Message}");
        }
    }
}