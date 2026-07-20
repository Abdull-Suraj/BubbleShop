using System.Text;
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Products.Queries.SearchProducts;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.SearchProducts;

public sealed class SearchProductsCommandHandler
    : IRequestHandler<SearchProductsCommand, Result<MessageResponse>>
{
    private readonly IMediator _mediator;

    public SearchProductsCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<MessageResponse>> Handle(
        SearchProductsCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SearchProductsQuery(
                Keyword: request.Keyword,
                BusinessId: request.BusinessId),
            cancellationToken);

        if (result.IsFailure)
        {
            return Result<MessageResponse>.Failure(
                result.Error ?? "An error occurred while searching for products.");
        }

        if (!result.Value.Items.Any())
        {
            return Result<MessageResponse>.Success(
                MessageResponse.Success("No products were found."));
        }

        var sb = new StringBuilder();

        sb.AppendLine("🛍 Products Found");
        sb.AppendLine();

        foreach (var product in result.Value.Items)
        {
            sb.AppendLine($"• {product.Name}");
            sb.AppendLine($"💰 {product.Price:C}");
            sb.AppendLine();
        }

        sb.AppendLine("Reply with the product name to order.");

        return Result<MessageResponse>.Success(
            MessageResponse.Success(sb.ToString()));
    }
}