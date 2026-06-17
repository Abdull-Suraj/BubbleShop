using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record WhatsAppMessageSentEvent : INotification
    {
        public string To { get; }
        public string Message { get; }
        public bool Success { get; }
        public DateTime OccurredOn { get; }

        public WhatsAppMessageSentEvent(string to, string message, bool success)
        {
            To = to;
            Message = message;
            Success = success;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
