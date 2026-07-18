// Application/Features/Feedback/Commands/ProvideFeedback/ProvideFeedbackCommand.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Feedback.Commands.ProvideFeedback;

public sealed record ProvideFeedbackCommand(
    string Channel,
    Guid CustomerId,
    Guid BusinessId,
    int Rating,
    string Feedback,
    string Message
) : IRequest<Result<MessageResponse>>;