using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.HandlePaymentWebhook;

public sealed class HandlePaymentWebhookCommandHandler : IRequestHandler<HandlePaymentWebhookCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HandlePaymentWebhookCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(HandlePaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get order
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure("Order not found.", "NotFound");
            }

            Payment? payment = order.Payment;

            if (payment is null)
            {
                payment = new Payment(
                    orderId: order.Id,
                    businessId: order.BusinessId,
                    amount: order.TotalAmount,
                    paymentMethod: PaymentMethod.CreditCard,
                    customerId: order.CustomerId,
                    provider: "Flutterwave"
                );

                await _paymentRepository.AddAsync(payment, cancellationToken);
            }

            payment.MarkSuccessful(request.TransactionId, request.GatewayResponse);

            order.AttachPayment(payment);

            // Update order status - Use PaymentReceived instead of Paid
            order.UpdateStatus(OrderStatus.PaymentReceived);

            // If you want to confirm the order automatically
            // order.Confirm();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to process payment webhook: {ex.Message}");
        }
    }
}