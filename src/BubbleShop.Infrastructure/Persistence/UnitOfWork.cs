using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;

        public UnitOfWork(AppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Get all entities with domain events
            var entities = _context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            // Collect all domain events
            var domainEvents = entities
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear events
            foreach (var entity in entities)
            {
                entity.ClearDomainEvents();
            }

            // Save changes
            var result = await _context.SaveChangesAsync(cancellationToken);

            // Dispatch events after save
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            return result;
        }
    }
}
