using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandHandler(IOrderRepository orderRepository, IPaymentService paymentService)
    : IRequestHandler<InitiatePaymentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<string>.Failure("Order not found.");
        }

        var paymentUrl = await paymentService.CreatePaymentLinkAsync(order.Id, order.TotalAmount, $"Payment for order {order.Id}", cancellationToken);
        return Result<string>.Success(paymentUrl);
    }
}
