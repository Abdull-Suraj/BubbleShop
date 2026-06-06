using BubbleShop.Application.Features.Customers.Queries.GetAllCustomers;
using BubbleShop.Application.Features.Customers.Queries.GetCustomerById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public sealed class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok((await mediator.Send(new GetAllCustomersQuery(), cancellationToken)).Value);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
