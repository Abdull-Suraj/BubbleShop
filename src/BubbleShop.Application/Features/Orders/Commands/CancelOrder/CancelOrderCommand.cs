using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest<Result>;
