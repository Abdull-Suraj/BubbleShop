using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;

public sealed class GetCustomerByWhatsAppNumberQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<GetCustomerByWhatsAppNumberQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByWhatsAppNumberQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByWhatsAppNumberAsync(request.WhatsAppNumber, cancellationToken);
        return customer is null
            ? Result<CustomerDto>.Failure("Customer not found.")
            : Result<CustomerDto>.Success(new CustomerDto(customer.Id, customer.WhatsAppNumber, customer.Name, customer.Email, customer.Address));
    }
}
