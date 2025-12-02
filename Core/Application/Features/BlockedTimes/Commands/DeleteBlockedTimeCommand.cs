using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.BlockedTimes.Commands;

public record DeleteBlockedTimeCommand(int Id) : IRequest<bool>;

public class DeleteBlockedTimeCommandHandler : IRequestHandler<DeleteBlockedTimeCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeleteBlockedTimeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteBlockedTimeCommand command, CancellationToken cancellationToken)
    {
        var blockedTime = await _context.BlockedTimes
     .FirstOrDefaultAsync(bt => bt.Id == command.Id, cancellationToken);

        if (blockedTime == null)
        {
            throw new KeyNotFoundException($"Blocked time with ID {command.Id} not found");
        }

        _context.BlockedTimes.Remove(blockedTime);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
