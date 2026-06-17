using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;

public sealed record GetCustomerByWhatsAppNumberQuery(
    string WhatsAppNumber,
    Guid BusinessId
) : IRequest<Result<CustomerDto>>;