using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record LowStockEvent : INotification
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public int CurrentStock { get; }
        public int Threshold { get; }
        public DateTime OccurredOn { get; }

        public LowStockEvent(Guid productId, string productName, int currentStock, int threshold = 10)
        {
            ProductId = productId;
            ProductName = productName;
            CurrentStock = currentStock;
            Threshold = threshold;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
