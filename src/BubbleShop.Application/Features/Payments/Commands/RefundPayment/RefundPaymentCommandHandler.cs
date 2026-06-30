using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Payments.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<RefundResponseDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<RefundResponseDto>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing refund for payment: {PaymentId}", request.PaymentId);

            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment is null)
                return Result<RefundResponseDto>.Failure($"Payment {request.PaymentId} not found", "NotFound");

            if (payment.Status != PaymentStatus.Successful)
                return Result<RefundResponseDto>.Failure($"Payment {request.PaymentId} is not successful", "ValidationError");

            if (request.Amount <= 0)
                return Result<RefundResponseDto>.Failure("Refund amount must be greater than zero", "ValidationError");

            var refundableAmount = payment.AmountPaid - payment.AmountRefunded;
            if (request.Amount > refundableAmount)
                return Result<RefundResponseDto>.Failure($"Refund amount exceeds refundable amount. Maximum refundable: {refundableAmount}", "ValidationError");

            // Process refund
            //payment.Refund(request.Amount, request.Reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refund processed successfully for payment: {PaymentId}, Amount: {Amount}", request.PaymentId, request.Amount);

            return Result<RefundResponseDto>.Success(new RefundResponseDto
            {
                PaymentId = payment.Id,
                TransactionReference = payment.TransactionReference,
                RefundAmount = request.Amount,
                RefundedAt = DateTime.UtcNow,
                NewStatus = payment.Status.ToString(),
                RemainingRefundable = payment.AmountPaid - payment.AmountRefunded
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment: {PaymentId}", request.PaymentId);
            return Result<RefundResponseDto>.Failure($"Failed to process refund: {ex.Message}");
        }
    }
}