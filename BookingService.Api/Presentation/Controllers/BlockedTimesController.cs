using BookingService.Api.Core.Application.Features.BlockedTimes.Commands;
using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using BookingService.Api.Core.Application.Features.BlockedTimes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingService.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("default")]
public class BlockedTimesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BlockedTimesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get blocked times for a specific resource (Admin only)
    /// </summary>
    [HttpGet("resource/{resourceId}")]
    [ProducesResponseType(typeof(List<BlockedTimeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<BlockedTimeDto>>> GetByResource(int resourceId)
    {
        var query = new GetBlockedTimesByResourceQuery(resourceId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a blocked time period for maintenance (Admin only)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BlockedTimeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BlockedTimeDto>> Create([FromBody] CreateBlockedTimeRequest request)
    {
        var command = new CreateBlockedTimeCommand(request);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByResource), new { resourceId = result.ResourceId }, result);
    }

    /// <summary>
    /// Delete a blocked time (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteBlockedTimeCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}
