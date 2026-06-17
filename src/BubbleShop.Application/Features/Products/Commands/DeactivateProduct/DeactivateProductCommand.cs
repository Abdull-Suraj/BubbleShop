// Application/Features/Products/Commands/DeactivateProduct/DeactivateProductCommand.cs
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.DeactivateProduct;

public sealed record DeactivateProductCommand(Guid ProductId) : IRequest<Result>;