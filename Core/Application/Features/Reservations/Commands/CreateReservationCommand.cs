using AutoMapper;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Reservations.Commands;

public record CreateReservationCommand(int UserId, CreateReservationRequest Request) : IRequest<ReservationDto>;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateReservationCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReservationDto> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Validate resource exists
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && r.IsActive, cancellationToken);

        if (resource == null)
        {
            throw new KeyNotFoundException($"Resource with ID {request.ResourceId} not found or inactive");
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
            throw new InvalidOperationException("The selected time slot overlaps with an existing reservation");
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
            throw new InvalidOperationException("The selected time slot is blocked for maintenance");
        }

        // Create reservation
        var reservation = _mapper.Map<Reservation>(request);
        reservation.UserId = command.UserId;
        reservation.Status = ReservationStatus.Active;
        reservation.CreatedAt = DateTime.UtcNow;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        // Load navigation properties for response
        await _context.Entry(reservation)
         .Reference(r => r.User)
  .LoadAsync(cancellationToken);
        await _context.Entry(reservation)
        .Reference(r => r.Resource)
   .LoadAsync(cancellationToken);

        return _mapper.Map<ReservationDto>(reservation);
    }
}
