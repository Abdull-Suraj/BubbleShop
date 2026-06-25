using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;

public sealed record CreateOrUpdateCustomerCommand(
    string WhatsAppNumber,
    string Name,
    string? Email = null,
    string? Address = null,
    Guid? BusinessId = null
) : IRequest<Result<CustomerDto>>;