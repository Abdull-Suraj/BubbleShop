
using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Features.Products.Queries;

public class GetProductPriceQuery : IRequest<IActionResult>
{
    public string ProductName { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
}