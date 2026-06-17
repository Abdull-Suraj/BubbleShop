using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Payments.Queries.GetPaymentHistory;

public sealed class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, Result<PagedResult<PaymentHistoryDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPaymentHistoryQueryHandler> _logger;

    public GetPaymentHistoryQueryHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetPaymentHistoryQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<PaymentHistoryDto>>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment history for business: {BusinessId}", request.BusinessId);

            var allPayments = await _paymentRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);

            // Apply date filters
            if (request.FromDate.HasValue)
                allPayments = allPayments.Where(p => p.CreatedAt >= request.FromDate.Value).ToList();
            if (request.ToDate.HasValue)
                allPayments = allPayments.Where(p => p.CreatedAt <= request.ToDate.Value).ToList();

            var totalCount = allPayments.Count;
            var pagedPayments = allPayments
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var paymentHistory = pagedPayments.Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                TransactionReference = p.TransactionReference,
                OrderId = p.OrderId,
                Amount = p.Amount,
                AmountPaid = p.AmountPaid,
                AmountRefunded = p.AmountRefunded,
                Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                PlatformFee = p.PlatformFee,
                PaymentGatewayFee = p.PaymentGatewayFee,
                BusinessEarnings = p.BusinessEarnings,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt
            }).ToList();

            var result = new PagedResult<PaymentHistoryDto>
            {
                Items = paymentHistory,
                TotalCount = totalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Result<PagedResult<PaymentHistoryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for business: {BusinessId}", request.BusinessId);
            return Result<PagedResult<PaymentHistoryDto>>.Failure($"Failed to retrieve payment history: {ex.Message}");
        }
    }
}