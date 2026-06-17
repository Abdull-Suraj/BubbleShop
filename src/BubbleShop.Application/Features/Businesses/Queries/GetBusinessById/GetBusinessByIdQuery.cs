using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Businesses.Queries.GetBusinessById;

public sealed record GetBusinessByIdQuery(Guid BusinessId) : IRequest<Result<BusinessDto>>;