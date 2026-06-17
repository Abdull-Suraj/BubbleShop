using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.Interfaces.Repositories
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
        Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default);
        Task SendOrderConfirmationAsync(string to, string orderNumber, decimal amount, CancellationToken cancellationToken = default);
        Task SendPaymentConfirmationAsync(string to, string transactionReference, decimal amount, CancellationToken cancellationToken = default);
        Task SendWelcomeEmailAsync(string to, string name, CancellationToken cancellationToken = default);
        Task SendPasswordResetAsync(string to, string resetToken, string resetLink, CancellationToken cancellationToken = default);
        Task SendDeliveryUpdateAsync(string to, string orderNumber, string status, string? trackingNumber = null, CancellationToken cancellationToken = default);
    }
}
