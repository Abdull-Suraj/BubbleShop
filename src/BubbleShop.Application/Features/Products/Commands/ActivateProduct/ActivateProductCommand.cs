// Application/Features/Products/Commands/ActivateProduct/ActivateProductCommand.cs
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.ActivateProduct;

public sealed record ActivateProductCommand(Guid ProductId) : IRequest<Result>;