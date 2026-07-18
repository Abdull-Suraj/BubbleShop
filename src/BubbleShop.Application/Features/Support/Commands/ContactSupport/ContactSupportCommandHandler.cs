
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Support.Commands.ContactSupport;

public sealed class ContactSupportCommandHandler : IRequestHandler<ContactSupportCommand, Result<MessageResponse>>
{
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ContactSupportCommandHandler> _logger;

    public ContactSupportCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<ContactSupportCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(ContactSupportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating support ticket for customer {CustomerId}", request.CustomerId);

            // Get customer
            var customer = await _customerRepository.GetByIdAsync(
                request.CustomerId,
                cancellationToken);

            if (customer is null)
            {
                return Result<MessageResponse>.Failure("Customer not found.");
            }
            if (customer is null)
            {
                // Create customer if not exists
                customer = new Customer(
                    businessId: request.BusinessId,
                    name: request.CustomerName ?? "Valued",
                    whatsappNumber: customer.WhatsAppNumber,
                    phoneNumber: customer.PhoneNumber,
                    email: null
                );
                await _customerRepository.AddAsync(customer, cancellationToken);
            }

            // Create support ticket
            var ticket = new SupportTicket(
             customer.Id,
             request.BusinessId,
             "Customer Support Request",
             request.Message,
             request.Channel,
             TicketCategory.General,
             TicketPriority.Normal);

            await _supportTicketRepository.AddAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = $"📧 **Support Ticket Created**\n\n" +
                          $"Thank you for reaching out, {customer.Name}! 🙏\n" +
                          $"Your support request has been received.\n\n" +
                          $"📋 **Ticket ID:** #{ticket.Id.ToString()[..8]}\n" +
                          $"📝 **Priority:** {ticket.Priority}\n" +
                          $"⏱️ **Estimated Response Time:** 1-2 hours\n\n" +
                          $"Our support team will contact you shortly.\n\n" +
                          $"In the meantime, you can:\n" +
                          $"• Reply with more details\n" +
                          $"• Ask me for self-help options\n\n" +
                          $"Is there anything else I can help you with? 😊";

            return Result<MessageResponse>.Success(
     MessageResponse.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support ticket for customer {CustomerId}", request.CustomerId);
            return Result<MessageResponse>.Failure($"Failed to create support ticket: {ex.Message}");
        }
    }
}