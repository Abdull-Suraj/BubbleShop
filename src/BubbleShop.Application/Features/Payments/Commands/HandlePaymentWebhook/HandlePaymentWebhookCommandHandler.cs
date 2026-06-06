using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.HandlePaymentWebhook;

public sealed class HandlePaymentWebhookCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<HandlePaymentWebhookCommand, Result>
{
    public async Task<Result> Handle(HandlePaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }

        var payment = order.Payment ?? Payment.Create(order.Id, "Stripe", order.TotalAmount);
        payment.MarkCompleted(request.TransactionId);
        order.AttachPayment(payment);
        order.UpdateStatus(OrderStatus.Paid);

        await orderRepository.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
