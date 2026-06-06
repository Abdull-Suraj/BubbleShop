using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.WhatsApp.Commands.SendMessage;

public sealed record SendMessageCommand(string ToNumber, string Message) : IRequest<Result>;
