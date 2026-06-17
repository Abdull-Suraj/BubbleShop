// Application/Features/Payments/Commands/InitiatePayment/InitiatePaymentCommandHandler.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BubbleShop.Application.Features.Payments.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, Result<PaymentInitiationResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InitiatePaymentCommandHandler> _logger;
    private readonly HttpClient _httpClient;

    public InitiatePaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<InitiatePaymentCommandHandler> logger,
        IHttpClientFactory httpClientFactory)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();

        _httpClient.BaseAddress = new Uri(_configuration["Flutterwave:BaseUrl"] ?? "https://api.flutterwave.com/v3");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["Flutterwave:SecretKey"]}");
    }

    public async Task<Result<PaymentInitiationResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Initiating {Provider} payment for order: {OrderId}", request.Provider, request.OrderId);

            // Get order
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result<PaymentInitiationResponse>.Failure($"Order {request.OrderId} not found", "NotFound");

            if (order.Status == OrderStatus.Cancelled)
                return Result<PaymentInitiationResponse>.Failure("Cannot initiate payment for cancelled order", "ValidationError");

            // Get business
            var business = await _businessRepository.GetByIdAsync(order.BusinessId, cancellationToken);
            if (business is null)
                return Result<PaymentInitiationResponse>.Failure("Business not found", "NotFound");

            // Generate transaction reference
            var transactionReference = GenerateTransactionReference(order.OrderNumber);

            // Create payment record
            var payment = new Payment(
                orderId: order.Id,
                businessId: order.BusinessId,
                amount: order.TotalAmount,
                paymentMethod: GetPaymentMethod(request.Provider),
                customerId: order.CustomerId
            );

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Initiate payment with provider
            var paymentLink = await InitiateWithProvider(request, order, transactionReference, cancellationToken);

            // Update payment with transaction reference
            payment.UpdateTransactionReference(transactionReference);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<PaymentInitiationResponse>.Success(new PaymentInitiationResponse
            {
                TransactionReference = transactionReference,
                PaymentLink = paymentLink,
                Status = "pending"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for order: {OrderId}", request.OrderId);
            return Result<PaymentInitiationResponse>.Failure($"Failed to initiate payment: {ex.Message}");
        }
    }

    private async Task<string> InitiateWithProvider(InitiatePaymentCommand request, Order order, string transactionReference, CancellationToken cancellationToken)
    {
        return request.Provider.ToLower() switch
        {
            "flutterwave" => await InitiateFlutterwavePayment(order, transactionReference, request.Currency, cancellationToken),
            "stripe" => await InitiateStripePayment(order, transactionReference, request.Currency, cancellationToken),
            "paystack" => await InitiatePaystackPayment(order, transactionReference, request.Currency, cancellationToken),
            _ => throw new ArgumentException($"Unsupported provider: {request.Provider}")
        };
    }

    private async Task<string> InitiateFlutterwavePayment(Order order, string transactionReference, string currency, CancellationToken cancellationToken)
    {
        var paymentData = new
        {
            tx_ref = transactionReference,
            amount = order.TotalAmount,
            currency = currency,
            redirect_url = $"{_configuration["AppBaseUrl"]}/api/payments/callback",
            customer = new
            {
                email = order.CustomerEmail ?? "customer@example.com",
                name = order.CustomerName,
                phone_number = order.CustomerPhone
            },
            customizations = new
            {
                title = "BubbleShop",
                description = $"Order #{order.OrderNumber}",
                logo = _configuration["AppLogoUrl"] ?? "https://bubbleshop.com/logo.png"
            },
            meta = new
            {
                order_id = order.Id.ToString(),
                order_number = order.OrderNumber
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(paymentData), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/payments", content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(responseContent);
        var status = doc.RootElement.GetProperty("status").GetString();

        if (status != "success")
        {
            var message = doc.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Payment initiation failed";
            throw new Exception($"Flutterwave: {message}");
        }

        return doc.RootElement.GetProperty("data").GetProperty("link").GetString() ?? string.Empty;
    }

    private async Task<string> InitiateStripePayment(Order order, string transactionReference, string currency, CancellationToken cancellationToken)
    {
        // Implement Stripe payment initiation
        await Task.CompletedTask;
        return $"https://stripe.com/pay/{transactionReference}";
    }

    private async Task<string> InitiatePaystackPayment(Order order, string transactionReference, string currency, CancellationToken cancellationToken)
    {
        // Implement Paystack payment initiation
        await Task.CompletedTask;
        return $"https://paystack.com/pay/{transactionReference}";
    }

    private PaymentMethod GetPaymentMethod(string provider)
    {
        return provider.ToLower() switch
        {
            "flutterwave" => PaymentMethod.CreditCard, 
            _ => PaymentMethod.CreditCard
        };
    }

    private string GenerateTransactionReference(string orderNumber)
    {
        return $"BS-{orderNumber}-{DateTime.Now:yyyyMMddHHmmss}";
    }
}