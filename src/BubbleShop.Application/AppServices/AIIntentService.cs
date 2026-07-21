// Application/Services/AIIntentService.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Common;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.AppServices;

public class AIIntentService : IAIIntentService
{
    private readonly ILogger<AIIntentService> _logger;

    public AIIntentService(ILogger<AIIntentService> logger)
    {
        _logger = logger;
    }

    public async Task<IntentResult> AnalyzeIntentAsync(string message, MessageContext context, CancellationToken cancellationToken = default)
    {
        var lowerMessage = message.ToLower().Trim();
        var result = new IntentResult
        {
            RawMessage = message,
            Parameters = new Dictionary<string, object>(),
            ExtractedEntities = new List<string>(),
            Confidence = 0.5m
        };

        // Greeting detection
        if (IsGreeting(lowerMessage))
        {
            result.Intent = Intent.JustChatting;
            result.Confidence = 0.95m;
            result.ResponseMessage = GetFriendlyGreeting();
            result.SuggestedResponses = new List<string>
            {
                "I want to buy something",
                "Show me your products",
                "What's the price of rice?"
            };
            return result;
        }

        // Thank you detection
        if (IsThankYou(lowerMessage))
        {
            result.Intent = Intent.ProvideFeedback;
            result.Confidence = 0.9m;
            result.ResponseMessage = GetThankYouResponse();
            result.SuggestedResponses = new List<string>
            {
                "You're welcome! Anything else?",
                "Thanks for shopping with us!"
            };
            return result;
        }

        // Order intent
        if (lowerMessage.Contains("want") || lowerMessage.Contains("buy") || lowerMessage.Contains("order") || lowerMessage.Contains("get"))
        {
            result = HandleOrderIntent(lowerMessage, message, result);
        }
        // Search intent
        else if (lowerMessage.Contains("show") || lowerMessage.Contains("search") || lowerMessage.Contains("find") ||
                 lowerMessage.Contains("what do you have") || lowerMessage.Contains("list"))
        {
            result = HandleSearchIntent(lowerMessage, message, result);
        }
        // Price inquiry
        else if (lowerMessage.Contains("price") || lowerMessage.Contains("how much") || lowerMessage.Contains("cost"))
        {
            result = HandlePriceIntent(lowerMessage, message, result);
        }
        // Stock inquiry
        else if (lowerMessage.Contains("stock") || lowerMessage.Contains("available") || lowerMessage.Contains("in stock"))
        {
            result = HandleStockIntent(lowerMessage, message, result);
        }
        // Track order
        else if (lowerMessage.Contains("track") || lowerMessage.Contains("where is my order") || lowerMessage.Contains("order status"))
        {
            result = HandleTrackOrderIntent(lowerMessage, message, result);
        }
        // Help
        else if (lowerMessage.Contains("help") || lowerMessage.Contains("support") || lowerMessage == "?" ||
                 lowerMessage.Contains("what can you do"))
        {
            result.Intent = Intent.GetHelp;
            result.Confidence = 0.9m;
            result.ResponseMessage = GetHelpMessage();
            result.SuggestedResponses = new List<string>
            {
                "I want to buy rice",
                "Show me your products",
                "How do I track my order?"
            };
        }
        // Goodbye
        else if (lowerMessage.Contains("bye") || lowerMessage.Contains("goodbye") || lowerMessage.Contains("see you"))
        {
            result.Intent = Intent.JustChatting;
            result.Confidence = 0.95m;
            result.ResponseMessage = GetGoodbyeResponse();
            result.SuggestedResponses = new List<string> { "Come back soon! 👋" };
        }
        // Unknown
        else
        {
            result = HandleUnknownIntent(message, result);
        }

        result.RequiresConfirmation = result.Intent == Intent.CreateOrder && !lowerMessage.Contains("confirm");
        return result;
    }

    public async Task<string> GenerateResponseAsync(IntentResult intent, MessageContext context, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(intent.ResponseMessage))
            return intent.ResponseMessage;

        return intent.Intent switch
        {
            Intent.CreateOrder => "I'll help you create an order! What would you like to buy? 🛒",
            Intent.SearchProduct => "Let me search for products for you! 🔍",
            Intent.GetProductPrice => "Let me check the price for you! 💰",
            Intent.CheckStock => "Let me check our inventory! 📦",
            Intent.TrackOrder => "I'll help you track your order! 🚚",
            Intent.GetHelp => GetHelpMessage(),
            _ => "How can I help you today? 😊"
        };
    }

    private IntentResult HandleOrderIntent(string lowerMessage, string originalMessage, IntentResult result)
    {
        result.Intent = Intent.CreateOrder;
        result.Confidence = 0.85m;

        var words = lowerMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int? quantity = null;
        string? product = null;

        for (int i = 0; i < words.Length; i++)
        {
            if (int.TryParse(words[i], out int qty))
            {
                quantity = qty;
                if (i + 1 < words.Length)
                    product = words[i + 1];
            }
            else if ((words[i] == "want" || words[i] == "buy" || words[i] == "get") && i + 1 < words.Length)
            {
                if (i + 2 < words.Length && int.TryParse(words[i + 1], out int qty2))
                {
                    quantity = qty2;
                    product = words[i + 2];
                }
                else
                {
                    product = words[i + 1];
                }
            }
        }

        quantity ??= 1;
        product ??= "that item";

        result.Parameters["Quantity"] = quantity;
        result.Parameters["ProductName"] = product;
        result.ExtractedEntities.Add(product);

        if (quantity > 5)
        {
            result.ResponseMessage = $"Wow! {quantity} of {product}? That's a bulk order! 🎉 Let me check if we have enough stock. Shall I create this order for you?";
        }
        else if (quantity == 1)
        {
            result.ResponseMessage = $"Great choice! 😊 So you want {quantity} {product}. Should I prepare that order for you?";
        }
        else
        {
            result.ResponseMessage = $"Awesome! {quantity} units of {product} coming right up! 🛒 Shall I create this order?";
        }

        result.SuggestedResponses = new List<string>
        {
            "Yes, create order!",
            "What's the total price?",
            "Cancel"
        };

        return result;
    }

    private IntentResult HandleSearchIntent(string lowerMessage, string originalMessage, IntentResult result)
    {
        result.Intent = Intent.SearchProduct;
        result.Confidence = 0.9m;

        var searchTerm = lowerMessage
            .Replace("show", "")
            .Replace("search", "")
            .Replace("find", "")
            .Replace("me", "")
            .Replace("what do you have", "")
            .Replace("list", "")
            .Trim();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            result.Parameters["SearchTerm"] = "all products";
            result.ResponseMessage = "Here's what we have in store today! 🏪\n\n" +
                                     "• Rice - Premium quality\n" +
                                     "• Beans - Fresh harvest\n" +
                                     "• Eggs - Farm fresh\n" +
                                     "• Meat - Well-packaged\n\n" +
                                     "What would you like to know more about? 😊";
        }
        else
        {
            result.Parameters["SearchTerm"] = searchTerm;
            result.ResponseMessage = $"Let me search for '{searchTerm}' for you! 🔍 One moment please...";
            result.ExtractedEntities.Add(searchTerm);
        }

        result.SuggestedResponses = new List<string>
        {
            "Tell me the price",
            "I want to buy some",
            "Show me something else"
        };

        return result;
    }

    private IntentResult HandlePriceIntent(string lowerMessage, string originalMessage, IntentResult result)
    {
        result.Intent = Intent.GetProductPrice;
        result.Confidence = 0.95m;

        var product = lowerMessage
            .Replace("how much", "")
            .Replace("is", "")
            .Replace("price", "")
            .Replace("cost", "")
            .Replace("of", "")
            .Trim();

        if (string.IsNullOrWhiteSpace(product))
            product = "this product";

        result.Parameters["ProductName"] = product;
        result.ExtractedEntities.Add(product);

        result.ResponseMessage = $"Let me check the price for {product}... 🏷️\n\n" +
                                 $"Our {product} is currently $25.99. That's a great price for this quality! 😊\n\n" +
                                 $"Would you like me to add it to your cart?";

        result.SuggestedResponses = new List<string>
        {
            $"Yes, add to cart",
            "That's a bit expensive",
            "Do you have a cheaper option?"
        };

        return result;
    }

    private IntentResult HandleStockIntent(string lowerMessage, string originalMessage, IntentResult result)
    {
        result.Intent = Intent.CheckStock;
        result.Confidence = 0.85m;

        var product = lowerMessage
            .Replace("stock", "")
            .Replace("available", "")
            .Replace("in stock", "")
            .Trim();

        if (string.IsNullOrWhiteSpace(product))
            product = "this product";

        result.Parameters["ProductName"] = product;
        result.ExtractedEntities.Add(product);

        result.ResponseMessage = $"Let me check the stock for {product}... 📦\n\n" +
                                 $"Yes! We have plenty of {product} in stock. ✅\n\n" +
                                 $"Would you like to order some?";

        result.SuggestedResponses = new List<string>
        {
            "I want to buy it",
            "What's the price?",
            "Show me something else"
        };

        return result;
    }

    private IntentResult HandleTrackOrderIntent(string lowerMessage, string originalMessage, IntentResult result)
    {
        result.Intent = Intent.TrackOrder;
        result.Confidence = 0.8m;

        var words = lowerMessage.Split(' ');
        string? orderNumber = null;

        foreach (var word in words)
        {
            if (word.StartsWith("ord") || word.StartsWith("#") || (word.Length > 5 && char.IsDigit(word[0])))
            {
                orderNumber = word;
                result.ExtractedEntities.Add(word);
                break;
            }
        }

        if (!string.IsNullOrEmpty(orderNumber))
        {
            result.Parameters["OrderNumber"] = orderNumber;
            result.ResponseMessage = $"Let me track order #{orderNumber} for you... 🚚\n\n" +
                                     $"Your order is being processed and should arrive soon!\n\n" +
                                     $"Want me to send you updates?";
        }
        else
        {
            result.ResponseMessage = $"I'd be happy to track your order! 🚚\n\n" +
                                     $"Please share your order number (e.g., ORD-12345) and I'll check the status for you.";
        }

        result.SuggestedResponses = new List<string>
        {
            "ORD-12345",
            "I don't have the order number",
            "When will it arrive?"
        };

        return result;
    }

    private IntentResult HandleUnknownIntent(string originalMessage, IntentResult result)
    {
        result.Intent = Intent.Unknown;
        result.Confidence = 0.3m;

        var friendlyResponses = new[]
        {
            $"Hmm, I want to help but I'm not quite sure what you mean by '{originalMessage}'. 🤔\n\nCould you tell me more? Are you looking to buy something or check prices?",
            $"I'm still learning! 😅 Did you want to buy something from our store?\n\nJust tell me 'I want [product]' and I'll help you out!",
            $"I think I might have misunderstood. Could you try rephrasing?\n\nFor example, you could say:\n• 'Show me rice'\n• 'How much is beans?'\n• 'I want to buy eggs'"
        };

        result.ResponseMessage = friendlyResponses[new Random().Next(friendlyResponses.Length)];
        result.SuggestedResponses = new List<string>
        {
            "I want to buy rice",
            "Show me your products",
            "How much is beans?"
        };

        return result;
    }

    private bool IsGreeting(string message)
    {
        var greetings = new[] { "hi", "hello", "hey", "good morning", "good afternoon", "good evening", "hola", "howdy" };
        return greetings.Any(g => message.StartsWith(g) || message == g);
    }

    private bool IsThankYou(string message)
    {
        var thanks = new[] { "thank", "thanks", "appreciate", "grateful", "thx" };
        return thanks.Any(t => message.Contains(t));
    }

    private string GetFriendlyGreeting()
    {
        var greetings = new[]
        {
            "Hey there! Welcome to Bubble Shop! 👋 How can I make your day better today?",
            "Hi! So glad to see you! 😊 What brings you to Bubble Shop today?",
            "Hello! Welcome, welcome! 🎉 I'm your store assistant. What can I help you find today?",
            "Hey! Great to see you! 🌟 Need any help with shopping today?"
        };
        return greetings[new Random().Next(greetings.Length)];
    }

    private string GetThankYouResponse()
    {
        var responses = new[]
        {
            "You're absolutely welcome! 😊 Come back anytime!",
            "Aww, thank you! 🥰 You're too kind! Let me know if you need anything else!",
            "Glad I could help! 🙏 Have a wonderful day!",
            "Thank YOU for shopping with us! 😊 You're awesome!"
        };
        return responses[new Random().Next(responses.Length)];
    }

    private string GetGoodbyeResponse()
    {
        var goodbyes = new[]
        {
            "Bye! Take care and come back soon! 👋",
            "See you later! 😊 Don't forget to come back for more goodies!",
            "Goodbye! Thanks for stopping by! 🎉 Have a great day!",
            "Take care! 👋 I'll be here when you need anything!"
        };
        return goodbyes[new Random().Next(goodbyes.Length)];
    }

    private string GetHelpMessage()
    {
        return "I can help you with:\n\n" +
               "📦 **Place an order** - Just say 'I want [product]'\n" +
               "💰 **Check prices** - Ask 'How much is [product]?'\n" +
               "🔍 **Find products** - Say 'Show me [product]'\n" +
               "📊 **Check stock** - Ask 'Do you have [product] in stock?'\n" +
               "🚚 **Track orders** - Say 'Track my order'\n\n" +
               "What would you like to do today? 😊";
    }
}