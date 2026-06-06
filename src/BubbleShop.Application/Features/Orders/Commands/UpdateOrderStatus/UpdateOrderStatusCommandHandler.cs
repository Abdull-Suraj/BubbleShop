using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }

        order.UpdateStatus(request.Status);
        await orderRepository.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
