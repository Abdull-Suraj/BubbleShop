using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Payments.Queries.GetPaymentStatus;

public sealed class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, Result<PaymentStatusDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPaymentStatusQueryHandler> _logger;

    public GetPaymentStatusQueryHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetPaymentStatusQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PaymentStatusDto>> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment status for: {TransactionReference}", request.TransactionReference);

            var payment = await _paymentRepository.GetByTransactionReferenceAsync(request.TransactionReference, cancellationToken);
            if (payment is null)
                return Result<PaymentStatusDto>.Failure($"Payment {request.TransactionReference} not found", "NotFound");

            var paymentStatus = new PaymentStatusDto
            {
                Id = payment.Id,
                TransactionReference = payment.TransactionReference,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                AmountPaid = payment.AmountPaid,
                AmountRefunded = payment.AmountRefunded,
                Status = payment.Status.ToString(),
                StatusDescription = GetStatusDescription(payment.Status),
                PaymentMethod = payment.PaymentMethod.ToString(),
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                RefundedAt = payment.RefundedAt
            };

            return Result<PaymentStatusDto>.Success(paymentStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for: {TransactionReference}", request.TransactionReference);
            return Result<PaymentStatusDto>.Failure($"Failed to get payment status: {ex.Message}");
        }
    }

    private string GetStatusDescription(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Payment pending",
        PaymentStatus.Processing => "Processing payment",
        PaymentStatus.Successful => "Payment successful",
        PaymentStatus.Failed => "Payment failed",
        PaymentStatus.Refunded => "Payment refunded",

        _ => status.ToString()
    };
}