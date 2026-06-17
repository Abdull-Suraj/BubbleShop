using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result>;