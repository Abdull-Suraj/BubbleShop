using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(ICustomerRepository customerRepository) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        return customer is null
            ? Result<CustomerDto>.Failure("Customer not found.")
            : Result<CustomerDto>.Success(new CustomerDto(customer.Id, customer.WhatsAppNumber, customer.Name, customer.Email, customer.Address));
    }
}
