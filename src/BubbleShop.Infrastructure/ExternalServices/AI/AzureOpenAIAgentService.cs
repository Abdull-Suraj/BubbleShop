//using Azure;
//using Azure.AI.OpenAI;
//using BubbleShop.Application.Common.Interfaces;
//using BubbleShop.Application.Features.Deliveries.Commands.ArrangeDelivery;
//using BubbleShop.Application.Features.Orders.Commands.CreateOrder;
//using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
//using BubbleShop.Application.Features.Payments.Commands.InitiatePayment;
//using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
//using BubbleShop.Application.Features.Products.Queries.SearchProducts;
//using BubbleShop.Infrastructure.Configuration;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using OpenAI.Chat;
//using System.Text.Json;
//using DomainChatMessage = BubbleShop.Domain.Entities.ChatMessage;

//namespace BubbleShop.Infrastructure.ExternalServices.AI;

//public sealed class AzureOpenAIAgentService : IAIAgentService
//{
//    private readonly ChatClient _chatClient;
//    private readonly AzureOpenAIOptions _options;
//    private readonly IMediator _mediator;
//    private readonly ILogger<AzureOpenAIAgentService> _logger;

//    public AzureOpenAIAgentService(IOptions<AzureOpenAIOptions> options, IMediator mediator, ILogger<AzureOpenAIAgentService> logger)
//    {
//        _options = options.Value;
//        _mediator = mediator;
//        _logger = logger;
//        var client = new AzureOpenAIClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));
//        _chatClient = client.GetChatClient(_options.DeploymentName);
//    }

//    public async Task<AgentResponse> ProcessAsync(List<DomainChatMessage> history, string newMessage, string customerId, CancellationToken cancellationToken = default)
//    {
//        var messages = new List<ChatMessage>();
//        foreach (var message in history)
//        {
//            messages.Add(message.Role == BubbleShop.Domain.Enums.ChatRole.User
//                ? new UserChatMessage(message.Content)
//                : new AssistantChatMessage(message.Content));
//        }

//        messages.Add(new UserChatMessage(newMessage));

//        var completionOptions = new ChatCompletionOptions
//        {
//            MaxOutputTokenCount = _options.MaxTokens,
//            Tools =
//            {
//                ChatTool.CreateFunctionTool("check_inventory", "Check product inventory", BinaryData.FromObjectAsJson(new { type = "object", properties = new { product_name = new { type = "string" }, size = new { type = "string" }, colour = new { type = "string" } } })),
//                ChatTool.CreateFunctionTool("create_order", "Create an order", BinaryData.FromObjectAsJson(new { type = "object", properties = new { customer_whatsapp_number = new { type = "string" }, items = new { type = "array" } } })),
//                ChatTool.CreateFunctionTool("get_order_status", "Get order status", BinaryData.FromObjectAsJson(new { type = "object", properties = new { order_id = new { type = "string" } } })),
//                ChatTool.CreateFunctionTool("initiate_payment", "Initiate payment", BinaryData.FromObjectAsJson(new { type = "object", properties = new { order_id = new { type = "string" } } })),
//                ChatTool.CreateFunctionTool("apply_discount", "Apply discount", BinaryData.FromObjectAsJson(new { type = "object", properties = new { order_id = new { type = "string" }, discount_code = new { type = "string" } } })),
//                ChatTool.CreateFunctionTool("arrange_delivery", "Arrange delivery", BinaryData.FromObjectAsJson(new { type = "object", properties = new { order_id = new { type = "string" }, address = new { type = "object" } } })),
//                ChatTool.CreateFunctionTool("get_product_list", "Get product list", BinaryData.FromObjectAsJson(new { type = "object", properties = new { category = new { type = "string" }, search_term = new { type = "string" } } }))
//            }
//        };

//        var completion = (await _chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken)).Value;

//        if (completion.FinishReason == ChatFinishReason.ToolCalls)
//        {
//            messages.Add(new AssistantChatMessage(completion));
//            foreach (var toolCall in completion.ToolCalls)
//            {
//                var toolOutput = await ExecuteToolAsync(toolCall, customerId, cancellationToken);
//                messages.Add(new ToolChatMessage(toolCall.Id, toolOutput));
//            }

//            completion = (await _chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken)).Value;
//        }

//        var reply = completion.Content.FirstOrDefault()?.Text ?? "How can I help?";
//        var updatedHistory = history.ToList();
//        updatedHistory.Add(new DomainChatMessage
//        {
//            Role = BubbleShop.Domain.Enums.ChatRole.Assistant,
//            Content = reply,
//            Timestamp = DateTime.UtcNow
//        });

//        return new AgentResponse
//        {
//            TextReply = reply,
//            UpdatedHistory = updatedHistory,
//            ToolCalls = completion.ToolCalls.Select(x => new ToolCall { FunctionName = x.FunctionName }).ToList()
//        };
//    }

//    private async Task<string> ExecuteToolAsync(ChatToolCall toolCall, string customerId, CancellationToken cancellationToken)
//    {
//        try
//        {
//            using var document = JsonDocument.Parse(toolCall.FunctionArguments);
//            var root = document.RootElement;

//            return toolCall.FunctionName switch
//            {
//                "check_inventory" => JsonSerializer.Serialize((await _mediator.Send(new SearchProductsQuery(root.GetProperty("product_name").GetString(), null), cancellationToken)).Value),
//                "create_order" => JsonSerializer.Serialize((await _mediator.Send(new CreateOrderCommand(Guid.Parse(customerId), []), cancellationToken)).Value),
//                "get_order_status" => JsonSerializer.Serialize((await _mediator.Send(new GetOrderByIdQuery(Guid.Parse(root.GetProperty("order_id").GetString()!)), cancellationToken)).Value),
//                "initiate_payment" => (await _mediator.Send(new InitiatePaymentCommand(Guid.Parse(root.GetProperty("order_id").GetString()!)), cancellationToken)).Value ?? string.Empty,
//                //"apply_discount" => "{\"applied\":false,\"reason\":\"Discount rules not configured\"}",
//                "arrange_delivery" => (await _mediator.Send(new ArrangeDeliveryCommand(Guid.Parse(root.GetProperty("order_id").GetString()!), "Customer", root.GetProperty("address").GetProperty("line1").GetString() ?? string.Empty, root.GetProperty("address").TryGetProperty("line2", out var line2) ? line2.GetString() : null, root.GetProperty("address").GetProperty("city").GetString() ?? string.Empty, root.GetProperty("address").GetProperty("postcode").GetString() ?? string.Empty, root.GetProperty("address").GetProperty("country").GetString() ?? string.Empty), cancellationToken)).Value ?? string.Empty,
//                "get_product_list" => JsonSerializer.Serialize((await _mediator.Send(new GetAllProductsQuery(), cancellationToken)).Value),
//                _ => "unsupported_tool"
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed tool execution: {Tool}", toolCall.FunctionName);
//            return "tool_execution_failed";
//        }
//    }
//}
