using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record OrderCancelledEvent : INotification
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; }

        public OrderCancelledEvent(Guid orderId, string orderNumber, string reason)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
