using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;

public sealed record GetCustomerByWhatsAppNumberQuery(string WhatsAppNumber) : IRequest<Result<CustomerDto>>;
