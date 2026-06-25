
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Common.Commands;

public class UnknownIntentCommand : IRequest<IActionResult>
{
    public string Message { get; set; } = string.Empty;
    public string SuggestedResponse { get; set; } = string.Empty;
}