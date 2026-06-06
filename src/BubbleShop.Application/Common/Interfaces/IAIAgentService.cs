using BubbleShop.Domain.Entities;

namespace BubbleShop.Application.Common.Interfaces;

public interface IAIAgentService
{
    Task<AgentResponse> ProcessAsync(List<ChatMessage> history, string newMessage, string customerId, CancellationToken cancellationToken = default);
}

public sealed class AgentResponse
{
    public string TextReply { get; init; } = string.Empty;
    public List<ToolCall> ToolCalls { get; init; } = [];
    public List<ChatMessage> UpdatedHistory { get; init; } = [];
}

public sealed class ToolCall
{
    public string FunctionName { get; init; } = string.Empty;
    public Dictionary<string, object?> Arguments { get; init; } = [];
}
