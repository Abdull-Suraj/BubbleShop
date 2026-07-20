using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Route("api/channels")]
[Produces("application/json")]
public class ChannelsController : ControllerBase
{
    private readonly IChannelFactory _channelFactory;
    private readonly IMessageRouter _messageRouter;
    private readonly ILogger<ChannelsController> _logger;

    public ChannelsController(
        IChannelFactory channelFactory,
        IMessageRouter messageRouter,
        ILogger<ChannelsController> logger)
    {
        _channelFactory = channelFactory;
        _messageRouter = messageRouter;
        _logger = logger;
    }

    /// <summary>
    /// Send message to a specific channel
    /// </summary>
    [HttpPost("{channelType}/send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage(
        string channelType,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ChannelType>(channelType, true, out var channel))
            return BadRequest(new { error = $"Invalid channel type: {channelType}" });

        var adapter = _channelFactory.GetChannelAdapter(channel);
        await adapter.SendMessageAsync(request.UserId, request.Message, cancellationToken);

        return Ok(new { success = true, message = "Message sent" });
    }

    /// <summary>
    /// Process incoming message from any channel
    /// </summary>
    [HttpPost("{channelType}/process")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessMessage(
        string channelType,
        [FromBody] ProcessMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ChannelType>(channelType, true, out var channel))
            return BadRequest(new { error = $"Invalid channel type: {channelType}" });

        var context = new MessageContext
        {
            Channel = channel,
            ChannelUserId = request.UserId,
            BusinessId = request.BusinessId,
            ReceivedAt = DateTime.UtcNow,
            Metadata = request.Metadata ?? new Dictionary<string, string>()
        };

        var response = await _messageRouter.ProcessIncomingMessageAsync(request.Message, context, cancellationToken);

        return Ok(new { success = true, response, conversationId = context.ConversationId });
    }
}

public class SendMessageRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ProcessMessageRequest
{
    public string UserId { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}