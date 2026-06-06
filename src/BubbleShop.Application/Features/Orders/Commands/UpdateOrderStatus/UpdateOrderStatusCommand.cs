using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Enums;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<Result>;
