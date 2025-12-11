using System.Security.Claims;
using BookingService.Api.Core.Application.Features.Auth.Commands;
using BookingService.Api.Core.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingService.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var ipAddress = GetIpAddress();
        var command = new RegisterCommand(request, ipAddress);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = GetIpAddress();
        var command = new LoginCommand(request, ipAddress);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = GetIpAddress();
        var command = new RefreshTokenCommand(request, ipAddress);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Revoke a refresh token (logout)
    /// </summary>
    [HttpPost("revoke-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var ipAddress = GetIpAddress();
        var command = new RevokeTokenCommand(request, ipAddress, userId);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest(new { message = "Token not found or already revoked" });
        }

        return Ok(new { message = "Token revoked successfully" });
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault()?.Trim() ?? "unknown";
        }
        
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }
}
