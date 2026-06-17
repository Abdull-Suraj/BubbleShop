using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record PaymentSuccessfulEvent : INotification
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public string TransactionReference { get; }
        public decimal Amount { get; }
        public DateTime OccurredOn { get; }

        public PaymentSuccessfulEvent(Guid paymentId, Guid orderId, string transactionReference, decimal amount)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            TransactionReference = transactionReference;
            Amount = amount;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
