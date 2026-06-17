using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.UnblockCustomer;

public sealed record UnblockCustomerCommand(Guid CustomerId) : IRequest<Result>;