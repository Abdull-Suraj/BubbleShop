using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record OrderPaidEvent : INotification
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public decimal Amount { get; }
        public DateTime OccurredOn { get; }

        public OrderPaidEvent(Guid orderId, string orderNumber, decimal amount)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            Amount = amount;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
