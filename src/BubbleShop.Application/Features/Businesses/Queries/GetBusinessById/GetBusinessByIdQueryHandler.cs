using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Businesses.Queries.GetBusinessById;

public sealed class GetBusinessByIdQueryHandler : IRequestHandler<GetBusinessByIdQuery, Result<BusinessDto>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ILogger<GetBusinessByIdQueryHandler> _logger;

    public GetBusinessByIdQueryHandler(
        IBusinessRepository businessRepository,
        ILogger<GetBusinessByIdQueryHandler> logger)
    {
        _businessRepository = businessRepository;
        _logger = logger;
    }

    public async Task<Result<BusinessDto>> Handle(GetBusinessByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting business by ID: {BusinessId}", request.BusinessId);

            var business = await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);
            if (business is null)
                return Result<BusinessDto>.Failure($"Business {request.BusinessId} not found", "NotFound");

            var businessDto = new BusinessDto
            {
                Id = business.Id,
                BusinessName = business.BusinessName,
                LegalName = business.LegalName,
                Email = business.Email,
                PhoneNumber = business.PhoneNumber,
                WhatsAppNumber = business.WhatsAppNumber,
                Address = business.Address,
                City = business.City,
                State = business.State,
                Country = business.Country,
                PostalCode = business.PostalCode,
                Status = business.Status.ToString(),
                WalletBalance = business.WalletBalance,
                Currency = business.Currency,
                IsVerified = business.IsVerified,
                VerifiedAt = business.VerifiedAt,
                CreatedAt = business.CreatedAt
            };

            return Result<BusinessDto>.Success(businessDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business by ID: {BusinessId}", request.BusinessId);
            return Result<BusinessDto>.Failure($"Failed to retrieve business: {ex.Message}");
        }
    }
}