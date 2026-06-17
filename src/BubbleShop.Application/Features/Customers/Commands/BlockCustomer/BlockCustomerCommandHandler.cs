// Application/Features/Customers/Commands/BlockCustomer/BlockCustomerCommandHandler.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Customers.Commands.BlockCustomer;

public sealed class BlockCustomerCommandHandler : IRequestHandler<BlockCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BlockCustomerCommandHandler> _logger;

    public BlockCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<BlockCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(BlockCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Blocking customer: {CustomerId}", request.CustomerId);

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure($"Customer {request.CustomerId} not found", "NotFound");

            // Now these methods exist
            customer.Block(request.Reason ?? "Blocked by admin");
            customer.AddNote($"Blocked by admin. Reason: {request.Reason ?? "No reason provided"}");

            await _customerRepository.UpdateAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer blocked successfully: {CustomerId}", request.CustomerId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking customer: {CustomerId}", request.CustomerId);
            return Result.Failure($"Failed to block customer: {ex.Message}");
        }
    }
}