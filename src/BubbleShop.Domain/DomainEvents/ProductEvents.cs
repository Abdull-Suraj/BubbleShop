using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record ProductCreatedEvent : INotification
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public Guid BusinessId { get; }
        public decimal Price { get; }
        public DateTime OccurredOn { get; }

        public ProductCreatedEvent(Guid productId, string productName, Guid businessId, decimal price)
        {
            ProductId = productId;
            ProductName = productName;
            BusinessId = businessId;
            Price = price;
            OccurredOn = DateTime.UtcNow;
        }
    }

}
