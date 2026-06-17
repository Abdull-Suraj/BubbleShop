using BubbleShop.Application.Features.Businesses.Commands.RegisterBusiness;
using BubbleShop.Application.Features.Businesses.Commands.UpdateBusiness;
using BubbleShop.Application.Features.Businesses.Queries.GetBusinessById;
using BubbleShop.Application.Features.Businesses.Queries.GetBusinessStats;
using BubbleShop.Application.Features.Businesses.Queries.GetBusinessWallet;
using BubbleShop.Application.Features.Businesses.Commands.LoginBusiness;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Route("api/businesses")]
[Produces("application/json")]
public sealed class BusinessesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BusinessesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new business
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterBusinessCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { businessId = result.Value });
    }

    /// <summary>
    /// Login business
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginBusinessCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    /// <summary>
    /// Get business by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBusinessByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Update business profile
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusinessCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { BusinessId = id }, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Get business statistics
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats(
        Guid id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBusinessStatsQuery(id, fromDate, toDate), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get business wallet balance
    /// </summary>
    [HttpGet("{id:guid}/wallet")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWallet(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBusinessWalletQuery(id), cancellationToken);
        return Ok(result.Value);
    }
}