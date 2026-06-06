using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }

        order.Cancel();
        await orderRepository.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
