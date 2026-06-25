using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);


public sealed record CreateOrderCommand(
    Guid BusinessId,
    Guid CustomerId,
    List<OrderItemInput> Items,
    string? CustomerName = null,
    string? CustomerWhatsApp = null,
    string? CustomerEmail = null,
    string? CustomerPhone = null,
    string? ShippingAddress = null,
    string? Channel = "API"
) : IRequest<Result<Guid>>;

public record OrderItemInput(
    Guid ProductId,
    int Quantity
);
