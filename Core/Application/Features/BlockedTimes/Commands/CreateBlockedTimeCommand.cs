using AutoMapper;
using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        // Validate resource exists
        var resourceExists = await _context.Resources
            .AnyAsync(r => r.Id == request.ResourceId && r.IsActive, cancellationToken);

        if (!resourceExists)
        {
            throw new KeyNotFoundException($"Resource with ID {request.ResourceId} not found or inactive");
        }

        // Create blocked time
        var blockedTime = _mapper.Map<BlockedTime>(request);
        blockedTime.CreatedAt = DateTime.UtcNow;

        _context.BlockedTimes.Add(blockedTime);
        await _context.SaveChangesAsync(cancellationToken);

        // Load navigation property for response
        await _context.Entry(blockedTime)
      .Reference(bt => bt.Resource)
     .LoadAsync(cancellationToken);

        return _mapper.Map<BlockedTimeDto>(blockedTime);
    }
}
