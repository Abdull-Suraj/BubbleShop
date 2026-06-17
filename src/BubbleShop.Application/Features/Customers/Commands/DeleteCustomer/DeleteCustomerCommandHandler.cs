using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Customers.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCustomerCommandHandler> _logger;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting customer: {CustomerId}", request.CustomerId);

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure($"Customer {request.CustomerId} not found", "NotFound");

            await _customerRepository.DeleteAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer deleted successfully: {CustomerId}", request.CustomerId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", request.CustomerId);
            return Result.Failure($"Failed to delete customer: {ex.Message}");
        }
    }
}