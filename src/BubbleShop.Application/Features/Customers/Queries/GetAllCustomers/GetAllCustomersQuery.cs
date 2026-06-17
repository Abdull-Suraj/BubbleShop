using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetAllCustomers;

public sealed record GetAllCustomersQuery : IRequest<Result<IReadOnlyList<CustomerDto>>>;
