using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : IRequest<Result>;