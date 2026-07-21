// Application/Features/Feedback/Commands/ProvideFeedback/ProvideFeedbackCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Feedback.Commands.ProvideFeedback;

public sealed class ProvideFeedbackCommandHandler : IRequestHandler<ProvideFeedbackCommand, Result<MessageResponse>>
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProvideFeedbackCommandHandler> _logger;

    public ProvideFeedbackCommandHandler(
        IFeedbackRepository feedbackRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProvideFeedbackCommandHandler> logger)
    {
        _feedbackRepository = feedbackRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(ProvideFeedbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received feedback from customer {CustomerId}", request.CustomerId);

            // Get customer
            var customer = await _customerRepository.GetByIdAsync(
           request.CustomerId,
           cancellationToken);

            if (customer is null)
            {
                return Result<MessageResponse>.Failure(
                    "Customer not found.",
                    "NotFound"
                );
            }

            // Create feedback
            var feedback = new BubbleShop.Domain.Entities.Feedback(
                customerId: customer.Id,
                businessId: request.BusinessId,
                rating: request.Rating,
                comment: request.Feedback,
                channel: request.Channel
            );

            await _feedbackRepository.AddAsync(feedback, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = $"🌟 **Thank You for Your Feedback!**\n\n";

            if (request.Rating >= 4)
            {
                response += "We're so glad you had a great experience! 😊\n";
                response += "Your feedback helps us serve you better.\n\n";
            }
            else if (request.Rating >= 3)
            {
                response += "Thank you for your feedback! 🙏\n";
                response += "We appreciate your honest opinion.\n\n";
            }
            else
            {
                response += "We're sorry to hear about your experience. 😔\n";
                response += "We'll work to improve. Thank you for letting us know.\n\n";
            }

            response += "Is there anything else we can help you with today? 😊";

            return Result<MessageResponse>.Success(
    MessageResponse.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving feedback from customer {CustomerId}", request.CustomerId);
            return Result<MessageResponse>.Failure($"Failed to save feedback: {ex.Message}");
        }
    }
}