using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Businesses.Commands.UpdateBusiness;

public sealed class UpdateBusinessCommandHandler : IRequestHandler<UpdateBusinessCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBusinessCommandHandler> _logger;

    public UpdateBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateBusinessCommandHandler> logger)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateBusinessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating business: {BusinessId}", request.BusinessId);

            var business = await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);
            if (business is null)
                return Result.Failure($"Business {request.BusinessId} not found", "NotFound");

            // Check email uniqueness if changed
            if (business.Email != request.Email)
            {
                var existingByEmail = await _businessRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingByEmail is not null)
                    return Result.Failure($"Email '{request.Email}' is already taken", "ValidationError");
            }

            // Check WhatsApp uniqueness if changed
            if (business.WhatsAppNumber != request.WhatsAppNumber)
            {
                var existingByWhatsApp = await _businessRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);
                if (existingByWhatsApp is not null)
                    return Result.Failure($"WhatsApp number '{request.WhatsAppNumber}' is already taken", "ValidationError");
            }

            // Update business
            business.UpdateProfile(
                businessName: request.BusinessName,
                email: request.Email,
                whatsAppNumber: request.WhatsAppNumber,
                phoneNumber: request.PhoneNumber,
                address: request.Address
            );

            if (!string.IsNullOrEmpty(request.LegalName))
                business.UpdateLegalName(request.LegalName);

            await _businessRepository.UpdateAsync(business, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Business updated successfully: {BusinessId}", request.BusinessId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business: {BusinessId}", request.BusinessId);
            return Result.Failure($"Failed to update business: {ex.Message}");
        }
    }
}