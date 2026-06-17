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

namespace BubbleShop.Application.Features.Businesses.EventHandlers
{
    public class BusinessVerifiedEventHandler : INotificationHandler<BusinessVerifiedEvent>
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IEmailService _emailService;
        private readonly ILogger<BusinessVerifiedEventHandler> _logger;

        public BusinessVerifiedEventHandler(
            IWhatsAppService whatsAppService,
            IEmailService emailService,
            ILogger<BusinessVerifiedEventHandler> logger)
        {
            _whatsAppService = whatsAppService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(BusinessVerifiedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Business verified: {BusinessName}", notification.BusinessName);

            // Send notification to business owner
            await _whatsAppService.SendMessageAsync(
                notification.BusinessId.ToString(),
                $"🎉 Congratulations! Your business '{notification.BusinessName}' has been verified!"
            );
        }
    }
}
