using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record ProductDeletedEvent : INotification
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public DateTime OccurredOn { get; }

        public ProductDeletedEvent(Guid productId, string productName)
        {
            ProductId = productId;
            ProductName = productName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
