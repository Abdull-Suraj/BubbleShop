using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;

public sealed class CreateOrUpdateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrUpdateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrUpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);
        if (customer is null)
        {
            customer = Customer.Create(request.WhatsAppNumber, request.Name, request.Email, request.Address);
            await customerRepository.AddAsync(customer, cancellationToken);
        }
        else
        {
            customer.Update(request.Name, request.Email, request.Address);
            await customerRepository.UpdateAsync(customer, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(customer.Id);
    }
}
