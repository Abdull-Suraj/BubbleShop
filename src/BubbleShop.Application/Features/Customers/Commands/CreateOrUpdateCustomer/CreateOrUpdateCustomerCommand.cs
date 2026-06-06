using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;

public sealed record CreateOrUpdateCustomerCommand(string WhatsAppNumber, string Name, string? Email, string? Address) : IRequest<Result<Guid>>;
