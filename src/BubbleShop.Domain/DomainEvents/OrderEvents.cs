using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record OrderCreatedEvent : INotification
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public Guid BusinessId { get; }
        public Guid CustomerId { get; }
        public decimal TotalAmount { get; }
        public DateTime OccurredOn { get; }

        public OrderCreatedEvent(Guid orderId, string orderNumber, Guid businessId, Guid customerId, decimal totalAmount)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            BusinessId = businessId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            OccurredOn = DateTime.UtcNow;
        }
    }

}
