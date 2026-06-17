using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;

public sealed class GetCustomerByWhatsAppNumberQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<GetCustomerByWhatsAppNumberQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByWhatsAppNumberQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);

        if (customer is null)
            return Result<CustomerDto>.Failure("Customer not found.");

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            WhatsAppNumber = customer.WhatsAppNumber,
            Name = customer.Name,
            Email = customer.Email,
            Address = customer.Address
        };

        return Result<CustomerDto>.Success(customerDto);
    }
}