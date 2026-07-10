using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerByWhatsAppNumber;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Customers.Queries.GetCustomersByBusiness;

public sealed class GetCustomersByBusinessQueryHandler : IRequestHandler<GetCustomersByBusinessQuery, Result<PagedResult<CustomerDto>>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<GetCustomersByBusinessQueryHandler> _logger;

    public GetCustomersByBusinessQueryHandler(
        ICustomerRepository customerRepository,
        ILogger<GetCustomersByBusinessQueryHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetCustomersByBusinessQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting customers for business: {BusinessId}", request.BusinessId);

            var allCustomers = await _customerRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                allCustomers = allCustomers.Where(c =>
                    c.Name?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) == true ||
                    c.Email?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) == true ||
                    c.WhatsAppNumber?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }

            var totalCount = allCustomers.Count;
            var pagedCustomers = allCustomers
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var customerDtos = pagedCustomers.Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                
                WhatsAppNumber = c.WhatsAppNumber,
                Email = c.Email,
                Address = c.Address,
                City = c.City,
                State = c.State,
                TotalOrders = c.TotalOrders,
                TotalSpent = c.TotalSpent,
                LastOrderDate = c.LastOrderDate,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            }).ToList();

            var result = new PagedResult<CustomerDto>
            {
                Items = customerDtos,
                TotalCount = totalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Result<PagedResult<CustomerDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers for business: {BusinessId}", request.BusinessId);
            return Result<PagedResult<CustomerDto>>.Failure($"Failed to retrieve customers: {ex.Message}");
        }
    }
}