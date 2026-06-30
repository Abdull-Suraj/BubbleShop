using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Application.AppServices;

public sealed class DummyAIAgentService : IAIAgentService
{
    public Task<AgentResponse> ProcessAsync(
        List<ChatMessage> history,
        string newMessage,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        history ??= new List<ChatMessage>();

        var updatedHistory = history.ToList();

        updatedHistory.Add(new ChatMessage
        {
            Role = ChatRole.User,
            Content = newMessage,
            Timestamp = DateTime.UtcNow
        });

        var toolCalls = DetectToolCalls(newMessage);

        var reply = GenerateReply(newMessage, toolCalls);

        updatedHistory.Add(new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = reply,
            Timestamp = DateTime.UtcNow
        });

        return Task.FromResult(new AgentResponse
        {
            TextReply = reply,
            ToolCalls = toolCalls,
            UpdatedHistory = updatedHistory
        });
    }

    private static string GenerateReply(string message, List<ToolCall> toolCalls)
    {
        if (toolCalls.Any())
        {
            return toolCalls[0].FunctionName switch
            {
                "SearchProducts" =>
                    "🔍 Let me search our catalogue for that product...",

                "GetProductPrice" =>
                    "💰 Let me check the latest price for you...",

                "CheckStock" =>
                    "📦 Let me check our current stock...",

                "AddToCart" =>
                    "🛒 I'll add that item to your cart.",

                "ViewCart" =>
                    "🛍️ Let me retrieve your shopping cart.",

                "Checkout" =>
                    "✅ Preparing your checkout...",

                "CreateOrder" =>
                    "📦 I'll create your order now.",

                "TrackOrder" =>
                    "🚚 Let me check your order status.",

                _ =>
                    "Processing your request..."
            };
        }

        var text = message.Trim().ToLowerInvariant();

        if (text.Contains("hello") || text.Contains("hi"))
            return "👋 Hello! Welcome to BubbleShop. How can I help you today?";

        if (text.Contains("help"))
            return """
I can help you with:

• Search products
• Check prices
• Check stock
• Add items to cart
• View your cart
• Checkout
• Place orders
• Track orders

What would you like to do?
""";

        return "🤖 I'm currently running in Demo Mode. I understood your message but couldn't determine a specific action.";
    }

    private static List<ToolCall> DetectToolCalls(string message)
    {
        var text = message.Trim().ToLowerInvariant();

        var toolCalls = new List<ToolCall>();

        if (text.Contains("price") || text.Contains("cost") || text.Contains("how much"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "GetProductPrice",
                Arguments = new Dictionary<string, object?>
                {
                    ["ProductName"] = ExtractProductName(text)
                }
            });

            return toolCalls;
        }

        if (text.Contains("search") ||
            text.Contains("find") ||
            text.Contains("show"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "SearchProducts",
                Arguments = new Dictionary<string, object?>
                {
                    ["SearchTerm"] = ExtractProductName(text)
                }
            });

            return toolCalls;
        }

        if (text.Contains("stock") ||
            text.Contains("available"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "CheckStock",
                Arguments = new Dictionary<string, object?>
                {
                    ["ProductName"] = ExtractProductName(text)
                }
            });

            return toolCalls;
        }

        if (text.Contains("add"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "AddToCart",
                Arguments = new Dictionary<string, object?>
                {
                    ["ProductName"] = ExtractProductName(text),
                    ["Quantity"] = 1
                }
            });

            return toolCalls;
        }

        if (text.Contains("cart"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "ViewCart",
                Arguments = new Dictionary<string, object?>()
            });

            return toolCalls;
        }

        if (text.Contains("checkout"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "Checkout",
                Arguments = new Dictionary<string, object?>()
            });

            return toolCalls;
        }

        if (text.Contains("buy") ||
            text.Contains("order"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "CreateOrder",
                Arguments = new Dictionary<string, object?>
                {
                    ["ProductName"] = ExtractProductName(text),
                    ["Quantity"] = 1
                }
            });

            return toolCalls;
        }

        if (text.Contains("track"))
        {
            toolCalls.Add(new ToolCall
            {
                FunctionName = "TrackOrder",
                Arguments = new Dictionary<string, object?>()
            });

            return toolCalls;
        }

        return toolCalls;
    }

    private static string ExtractProductName(string message)
    {
        var stopWords = new[]
        {
            "buy",
            "order",
            "price",
            "cost",
            "how",
            "much",
            "show",
            "find",
            "search",
            "for",
            "of",
            "the",
            "please",
            "check",
            "stock",
            "available",
            "add",
            "to",
            "cart",
            "track",
            "my"
        };

        var words = message
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !stopWords.Contains(w))
            .ToList();

        return words.Any()
            ? string.Join(" ", words)
            : string.Empty;
    }
}