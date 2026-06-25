using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;

public sealed class CreateOrUpdateCustomerCommandHandler
    : IRequestHandler<CreateOrUpdateCustomerCommand, Result<CustomerDto>>
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

    public async Task<Result<CustomerDto>> Handle(CreateOrUpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if customer exists
            var customer = await _customerRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);

            if (customer is null)
            {
                // Create new customer
                customer = new Customer(
                    whatsappNumber: request.WhatsAppNumber,
                    name: request.Name,
                    email: request.Email
                );

                if (!string.IsNullOrWhiteSpace(request.Address))
                {
                    customer.UpdateAddress(request.Address);
                }
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

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                WhatsAppNumber = customer.WhatsAppNumber,
                Email = customer.Email,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                TotalOrders = customer.TotalOrders,
                TotalSpent = customer.TotalSpent,
                LastOrderDate = customer.LastOrderDate,
                Status = customer.Status.ToString(),
                CreatedAt = customer.CreatedAt
            };

            return Result<CustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            return Result<CustomerDto>.Failure($"Failed to create or update customer: {ex.Message}");
        }
    }
}