// Application/Features/Payments/Queries/GetCustomerPayments/GetCustomerPaymentsQueryHandler.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Payments.Queries.GetCustomerPayments;

public sealed class GetCustomerPaymentsQueryHandler : IRequestHandler<GetCustomerPaymentsQuery, Result<PagedResult<PaymentHistoryDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetCustomerPaymentsQueryHandler> _logger;

    public GetCustomerPaymentsQueryHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetCustomerPaymentsQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<PaymentHistoryDto>>> Handle(GetCustomerPaymentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payments for customer: {CustomerId}", request.CustomerId);

            var payments = await _paymentRepository.GetPaymentsByCustomerIdAsync(request.CustomerId, cancellationToken);
            var paymentList = payments.ToList();

            var totalCount = paymentList.Count;
            var pagedPayments = paymentList
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
                PaidAt = p.PaidAt,
              
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
            _logger.LogError(ex, "Error getting payments for customer: {CustomerId}", request.CustomerId);
            return Result<PagedResult<PaymentHistoryDto>>.Failure($"Failed to retrieve payments: {ex.Message}");
        }
    }
}