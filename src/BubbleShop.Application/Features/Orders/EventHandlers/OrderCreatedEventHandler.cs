using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.DomainEvents;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Orders.EventHandlers
{
    public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(
            IWhatsAppService whatsAppService,
            IEmailService emailService,
            ILogger<OrderCreatedEventHandler> logger)
        {
            _whatsAppService = whatsAppService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling OrderCreatedEvent for OrderId: {OrderId}", notification.OrderId);

            // Send WhatsApp notification
            await _whatsAppService.SendMessageAsync(
                notification.CustomerId.ToString(),
                $"Order #{notification.OrderNumber} created! Total: {notification.TotalAmount:C}"
            );

            // Send email notification
            await _emailService.SendOrderConfirmationAsync(
                notification.CustomerId.ToString(),
                notification.OrderNumber,
                notification.TotalAmount
            );
        }
    }
}
