using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Businesses.Commands.LoginBusiness;

public sealed record LoginBusinessCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResponseDto>>;