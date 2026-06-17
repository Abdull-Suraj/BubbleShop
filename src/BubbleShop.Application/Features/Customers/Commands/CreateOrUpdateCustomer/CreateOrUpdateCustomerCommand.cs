// Application/Features/Customers/Commands/CreateOrUpdateCustomer/CreateOrUpdateCustomerCommand.cs
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;

public sealed record CreateOrUpdateCustomerCommand(
    string WhatsAppNumber,
    string Name,
    string? Email = null,
    string? Address = null,
    Guid? BusinessId = null  // Add BusinessId
) : IRequest<Result<Guid>>;