using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Resources.Commands;

public record DeleteResourceCommand(int Id) : IRequest<bool>;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeleteResourceCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteResourceCommand command, CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
           .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (resource == null)
        {
            throw new KeyNotFoundException($"Resource with ID {command.Id} not found");
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
