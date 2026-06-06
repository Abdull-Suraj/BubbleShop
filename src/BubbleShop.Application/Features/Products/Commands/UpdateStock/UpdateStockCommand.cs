using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.UpdateStock;

public sealed record UpdateStockCommand(Guid ProductId, int Quantity) : IRequest<Result>;
