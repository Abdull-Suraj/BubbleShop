using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.BlockCustomer;

public sealed record BlockCustomerCommand(Guid CustomerId, string? Reason = null) : IRequest<Result>;