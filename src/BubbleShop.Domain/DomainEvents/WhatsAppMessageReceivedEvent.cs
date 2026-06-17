using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record WhatsAppMessageReceivedEvent : INotification
    {
        public string From { get; }
        public string To { get; }
        public string Message { get; }
        public string BusinessId { get; }
        public DateTime OccurredOn { get; }

        public WhatsAppMessageReceivedEvent(string from, string to, string message, string businessId)
        {
            From = from;
            To = to;
            Message = message;
            BusinessId = businessId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
