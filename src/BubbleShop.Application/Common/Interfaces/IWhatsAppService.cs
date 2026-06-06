namespace BubbleShop.Application.Common.Interfaces;

public interface IWhatsAppService
{
    Task SendMessageAsync(string toNumber, string message, CancellationToken cancellationToken = default);
    Task SendMessageWithButtonsAsync(string toNumber, string message, IReadOnlyCollection<string> buttons, CancellationToken cancellationToken = default);
}
