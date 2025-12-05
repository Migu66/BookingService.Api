using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Reservations.Queries;

public record CheckAvailabilityQuery(AvailabilityRequest Request) : IRequest<AvailabilityResponse>;

public class CheckAvailabilityQueryHandler : IRequestHandler<CheckAvailabilityQuery, AvailabilityResponse>
{
    private readonly ApplicationDbContext _context;

    public CheckAvailabilityQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AvailabilityResponse> Handle(CheckAvailabilityQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;

        // Check if resource exists and is active
        var resourceExists = await _context.Resources
     .AnyAsync(r => r.Id == request.ResourceId && r.IsActive, cancellationToken);

        if (!resourceExists)
        {
            return new AvailabilityResponse
            {
                IsAvailable = false,
                Message = "Resource not found or inactive"
            };
        }

        // Check for overlapping reservations
        var hasOverlap = await _context.Reservations
            .AnyAsync(r =>
    r.ResourceId == request.ResourceId &&
       r.Status == ReservationStatus.Active &&
                r.StartTime < request.EndTime &&
     r.EndTime > request.StartTime,
       cancellationToken);

        if (hasOverlap)
        {
            return new AvailabilityResponse
            {
                IsAvailable = false,
                Message = "Time slot overlaps with an existing reservation"
            };
        }

        // Check for blocked times
        var hasBlockedTime = await _context.BlockedTimes
            .AnyAsync(bt =>
            bt.ResourceId == request.ResourceId &&
         bt.StartTime < request.EndTime &&
   bt.EndTime > request.StartTime,
             cancellationToken);

        if (hasBlockedTime)
        {
            return new AvailabilityResponse
            {
                IsAvailable = false,
                Message = "Time slot is blocked for maintenance"
            };
        }

        return new AvailabilityResponse
        {
            IsAvailable = true,
            Message = "Time slot is available"
        };
    }
}
