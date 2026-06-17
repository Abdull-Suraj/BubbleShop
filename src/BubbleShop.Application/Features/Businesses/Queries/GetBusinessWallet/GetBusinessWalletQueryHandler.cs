using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Businesses.Queries.GetBusinessWallet;

public sealed class GetBusinessWalletQueryHandler : IRequestHandler<GetBusinessWalletQuery, Result<BusinessWalletDto>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetBusinessWalletQueryHandler> _logger;

    public GetBusinessWalletQueryHandler(
        IBusinessRepository businessRepository,
        IPaymentRepository paymentRepository,
        ILogger<GetBusinessWalletQueryHandler> logger)
    {
        _businessRepository = businessRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<BusinessWalletDto>> Handle(GetBusinessWalletQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting wallet for business: {BusinessId}", request.BusinessId);

            var business = await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);
            if (business is null)
                return Result<BusinessWalletDto>.Failure($"Business {request.BusinessId} not found", "NotFound");

            // Get pending payouts
            var payments = await _paymentRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);
            var pendingPayments = payments.Where(p => p.Status == PaymentStatus.Successful && p.BusinessEarnings > 0).Sum(p => p.BusinessEarnings);
            var availableBalance = business.WalletBalance;
            var totalEarned = payments.Where(p => p.Status == PaymentStatus.Successful).Sum(p => p.BusinessEarnings);

            var wallet = new BusinessWalletDto
            {
                BusinessId = business.Id,
                BusinessName = business.BusinessName,
                Currency = business.Currency,
                AvailableBalance = availableBalance,
                PendingSettlement = pendingPayments,
                TotalEarned = totalEarned,
                LastUpdated = business.LastModifiedAt ?? business.CreatedAt
            };

            return Result<BusinessWalletDto>.Success(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet for business: {BusinessId}", request.BusinessId);
            return Result<BusinessWalletDto>.Failure($"Failed to retrieve wallet: {ex.Message}");
        }
    }
}