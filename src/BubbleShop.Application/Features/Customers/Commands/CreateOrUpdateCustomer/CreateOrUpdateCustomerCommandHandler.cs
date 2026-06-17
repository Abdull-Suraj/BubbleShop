using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;

public sealed class CreateOrUpdateCustomerCommandHandler : IRequestHandler<CreateOrUpdateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrUpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateOrUpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if customer exists
            var customer = await _customerRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);

            if (customer is null)
            {
                // Create new customer
                customer = Customer.Create(
                    whatsappNumber: request.WhatsAppNumber,
                    name: request.Name,
                    email: request.Email,
                    address: request.Address
                );

                // Assign to business if BusinessId is provided
                if (request.BusinessId.HasValue)
                {
                    customer.AssignToBusiness(request.BusinessId.Value);
                }

                await _customerRepository.AddAsync(customer, cancellationToken);
            }
            else
            {
                // Update existing customer
                customer.Update(
                    name: request.Name,
                    email: request.Email,
                    address: request.Address
                );

                // Update BusinessId if provided
                if (request.BusinessId.HasValue && customer.BusinessId != request.BusinessId.Value)
                {
                    customer.AssignToBusiness(request.BusinessId.Value);
                }

                await _customerRepository.UpdateAsync(customer, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(customer.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create or update customer: {ex.Message}");
        }
    }
}