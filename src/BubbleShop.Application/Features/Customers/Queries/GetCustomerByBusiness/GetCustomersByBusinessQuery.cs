using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomersByBusiness;

public sealed record GetCustomersByBusinessQuery(
    Guid BusinessId,
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null
) : IRequest<Result<PagedResult<CustomerDto>>>;