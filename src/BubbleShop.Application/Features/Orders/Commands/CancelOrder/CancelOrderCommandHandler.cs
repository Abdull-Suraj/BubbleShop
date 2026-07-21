// Application/Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelOrderCommandHandler> _logger;
    private readonly ICustomerRepository _customerRepository;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICustomerRepository customerRepository,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order == null)
            return Result<bool>.Failure("Order not found");


        var customer = await _customerRepository.GetByWhatsAppNumberAsync(
            request.ChannelUserId,
            request.BusinessId,
            cancellationToken);


        if (customer == null)
        {
            return Result<bool>.Failure(
                "Customer account not found.");
        }


        if (order.CustomerId != customer.Id)
        {
            return Result<bool>.Failure(
                "You are not allowed to cancel this order.");
        }


        if (!order.CanBeCancelled())
        {
            return Result<bool>.Failure(
                "This order cannot be cancelled at this stage.");
        }


        order.Cancel(
            request.Reason ?? "Customer requested cancellation");


        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return Result<bool>.Success(true);
    }
}