using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<CustomerDto>>;
