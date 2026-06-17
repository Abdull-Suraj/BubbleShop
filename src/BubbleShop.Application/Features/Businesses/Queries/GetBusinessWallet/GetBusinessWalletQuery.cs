using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Businesses.Queries.GetBusinessWallet;

public sealed record GetBusinessWalletQuery(Guid BusinessId) : IRequest<Result<BusinessWalletDto>>;