using AutoMapper;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace BookingService.Api.Core.Application.Features.Reservations.Commands;

public record CreateReservationCommand(int UserId, CreateReservationRequest Request) : IRequest<ReservationDto>;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private const int MaxRetryAttempts = 3;

    public CreateReservationCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReservationDto> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Use Serializable isolation level to prevent race conditions
        var executionStrategy = _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            try
            {
                // Validate resource exists (with lock)
                var resource = await _context.Resources
                    .FromSqlRaw("SELECT * FROM \"Resources\" WHERE \"Id\" = {0} AND \"IsActive\" = true FOR UPDATE", request.ResourceId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (resource == null)
                {
                    throw new KeyNotFoundException($"Resource with ID {request.ResourceId} not found or inactive");
                }

                // Check for overlapping reservations with row-level lock
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

                await transaction.CommitAsync(cancellationToken);

                // Load navigation properties for response (outside transaction)
                await _context.Entry(reservation)
                    .Reference(r => r.User)
                    .LoadAsync(cancellationToken);
                await _context.Entry(reservation)
                    .Reference(r => r.Resource)
                    .LoadAsync(cancellationToken);

                return _mapper.Map<ReservationDto>(reservation);
            }
            catch (DbUpdateException ex) when (IsConcurrencyException(ex))
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(
                    "The selected time slot was just reserved by another user. Please try a different time.", ex);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private static bool IsConcurrencyException(DbUpdateException ex)
    {
        // Check for PostgreSQL serialization failure or deadlock
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("could not serialize") ||
               message.Contains("deadlock") ||
               message.Contains("duplicate key") ||
               message.Contains("23505"); // PostgreSQL unique violation
    }
}
