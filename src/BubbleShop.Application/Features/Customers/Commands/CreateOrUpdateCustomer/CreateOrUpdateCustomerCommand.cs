using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;

public sealed record CreateOrUpdateCustomerCommand(
    string WhatsAppNumber,
    string Name,
    string? PhoneNumber = null,
    string? Email = null,
    string? Address = null,
    string? City = null,
    string? State = null
) : IRequest<Result<CustomerDto>>;