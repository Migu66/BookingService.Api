using AutoMapper;
using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BookingService.Api.Core.Application.Features.BlockedTimes.Commands;

public record CreateBlockedTimeCommand(CreateBlockedTimeRequest Request) : IRequest<BlockedTimeDto>;

public class CreateBlockedTimeCommandHandler : IRequestHandler<CreateBlockedTimeCommand, BlockedTimeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateBlockedTimeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BlockedTimeDto> Handle(CreateBlockedTimeCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var executionStrategy = _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            try
            {
                // Validate resource exists with lock
                var resource = await _context.Resources
                    .FromSqlRaw("SELECT * FROM \"Resources\" WHERE \"Id\" = {0} AND \"IsActive\" = true FOR UPDATE", request.ResourceId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (resource == null)
                {
                    throw new KeyNotFoundException($"Resource with ID {request.ResourceId} not found or inactive");
                }

                // Check for overlapping active reservations
                var hasOverlappingReservation = await _context.Reservations
                    .AnyAsync(r =>
                        r.ResourceId == request.ResourceId &&
                        r.Status == ReservationStatus.Active &&
                        r.StartTime < request.EndTime &&
                        r.EndTime > request.StartTime,
                        cancellationToken);

                if (hasOverlappingReservation)
                {
                    throw new InvalidOperationException(
                        "Cannot block this time slot: there are active reservations during this period");
                }

                // Check for overlapping blocked times
                var hasOverlappingBlockedTime = await _context.BlockedTimes
                    .AnyAsync(bt =>
                        bt.ResourceId == request.ResourceId &&
                        bt.StartTime < request.EndTime &&
                        bt.EndTime > request.StartTime,
                        cancellationToken);

                if (hasOverlappingBlockedTime)
                {
                    throw new InvalidOperationException(
                        "This time slot already overlaps with an existing blocked time");
                }

                // Create blocked time
                var blockedTime = _mapper.Map<BlockedTime>(request);
                blockedTime.CreatedAt = DateTime.UtcNow;

                _context.BlockedTimes.Add(blockedTime);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                // Load navigation property for response
                await _context.Entry(blockedTime)
                    .Reference(bt => bt.Resource)
                    .LoadAsync(cancellationToken);

                return _mapper.Map<BlockedTimeDto>(blockedTime);
            }
            catch (DbUpdateException ex) when (IsConcurrencyException(ex))
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(
                    "A concurrent operation modified this time slot. Please try again.", ex);
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
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("could not serialize") ||
               message.Contains("deadlock") ||
               message.Contains("duplicate key") ||
               message.Contains("23505");
    }
}
