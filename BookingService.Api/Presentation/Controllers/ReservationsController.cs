using System.Security.Claims;
using BookingService.Api.Core.Application.Features.Reservations.Commands;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Application.Features.Reservations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    /// <summary>
    /// Get all reservations (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ReservationDto>>> GetAll()
    {
        var query = new GetAllReservationsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get my reservations
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservationDto>>> GetMyReservations()
    {
        var userId = GetUserId();
        var query = new GetMyReservationsQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get reservation by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetById(int id)
    {
        var userId = GetUserId();
        var isAdmin = IsAdmin();
        var query = new GetReservationByIdQuery(id, userId, isAdmin);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Check availability for a resource
    /// </summary>
    [HttpPost("availability")]
    [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AvailabilityResponse>> CheckAvailability([FromBody] AvailabilityRequest request)
    {
        var query = new CheckAvailabilityQuery(request);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new reservation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationRequest request)
    {
        var userId = GetUserId();
        var command = new CreateReservationCommand(userId, request);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancel(int id)
    {
        var userId = GetUserId();
        var isAdmin = IsAdmin();
        var command = new CancelReservationCommand(id, userId, isAdmin);
        await _mediator.Send(command);
        return NoContent();
    }
}
