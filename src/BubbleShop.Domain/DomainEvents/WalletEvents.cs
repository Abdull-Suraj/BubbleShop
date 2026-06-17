using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record WalletCreditedEvent : INotification
    {
        public Guid BusinessId { get; }
        public decimal Amount { get; }
        public decimal NewBalance { get; }
        public string? Description { get; }
        public DateTime OccurredOn { get; }

        public WalletCreditedEvent(Guid businessId, decimal amount, decimal newBalance, string? description = null)
        {
            BusinessId = businessId;
            Amount = amount;
            NewBalance = newBalance;
            Description = description;
            OccurredOn = DateTime.UtcNow;
        }
    }

}
