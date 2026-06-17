
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Customers.Commands.UnblockCustomer;

public sealed class UnblockCustomerCommandHandler : IRequestHandler<UnblockCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnblockCustomerCommandHandler> _logger;

    public UnblockCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<UnblockCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UnblockCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Unblocking customer: {CustomerId}", request.CustomerId);

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure($"Customer {request.CustomerId} not found", "NotFound");

            if (customer.Status != CustomerStatus.Blocked)
                return Result.Failure("Customer is not blocked", "ValidationError");

            customer.Unblock();
            await _customerRepository.UpdateAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer unblocked successfully: {CustomerId}", request.CustomerId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking customer: {CustomerId}", request.CustomerId);
            return Result.Failure($"Failed to unblock customer: {ex.Message}");
        }
    }
}