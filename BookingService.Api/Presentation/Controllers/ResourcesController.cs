using BookingService.Api.Core.Application.Common.Models;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Core.Application.Features.Resources.Commands;
using BookingService.Api.Core.Application.Features.Resources.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingService.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("default")]
public class ResourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all active resources
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ResourceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ResourceDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAllResourcesQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get resource by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto>> GetById(int id)
    {
        var query = new GetResourceByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new resource (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ResourceDto>> Create([FromBody] CreateResourceRequest request)
    {
        var command = new CreateResourceCommand(request);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing resource (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto>> Update(int id, [FromBody] UpdateResourceRequest request)
    {
        var command = new UpdateResourceCommand(id, request);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete a resource (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteResourceCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}
