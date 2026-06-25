
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Features.Products.Queries;

public class CheckStockQuery : IRequest<IActionResult>
{
    public string ProductName { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
}