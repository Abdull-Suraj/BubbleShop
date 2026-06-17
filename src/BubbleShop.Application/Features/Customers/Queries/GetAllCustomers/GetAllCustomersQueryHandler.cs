using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Customers.Queries.GetAllCustomers;

public sealed class GetAllCustomersQueryHandler(ICustomerRepository customerRepository) : IRequestHandler<GetAllCustomersQuery, Result<IReadOnlyList<CustomerDto>>>
{
    public async Task<Result<IReadOnlyList<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await customerRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<CustomerDto>>.Success(customers.Select(x => new CustomerDto(x.Id, x.WhatsAppNumber, x.Name, x.Email, x.Address)).ToList());
    }
}
