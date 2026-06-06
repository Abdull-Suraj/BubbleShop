namespace BubbleShop.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentLinkAsync(Guid orderId, decimal amount, string description, CancellationToken cancellationToken = default);
}
