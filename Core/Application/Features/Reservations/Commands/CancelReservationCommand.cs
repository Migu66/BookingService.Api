using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Reservations.Commands;

public record CancelReservationCommand(int ReservationId, int UserId, bool IsAdmin) : IRequest<bool>;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public CancelReservationCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
              .FirstOrDefaultAsync(r => r.Id == command.ReservationId, cancellationToken);

        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID {command.ReservationId} not found");
        }

        // Check authorization: user can only cancel their own reservations, unless they're admin
        if (!command.IsAdmin && reservation.UserId != command.UserId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own reservations");
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            throw new InvalidOperationException($"Cannot cancel reservation with status {reservation.Status}");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
