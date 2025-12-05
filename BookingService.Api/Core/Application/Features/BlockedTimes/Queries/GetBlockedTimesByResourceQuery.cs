using AutoMapper;
using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.BlockedTimes.Queries;

public record GetBlockedTimesByResourceQuery(int ResourceId) : IRequest<List<BlockedTimeDto>>;

public class GetBlockedTimesByResourceQueryHandler : IRequestHandler<GetBlockedTimesByResourceQuery, List<BlockedTimeDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBlockedTimesByResourceQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BlockedTimeDto>> Handle(GetBlockedTimesByResourceQuery query, CancellationToken cancellationToken)
    {
        var blockedTimes = await _context.BlockedTimes
             .Include(bt => bt.Resource)
        .Where(bt => bt.ResourceId == query.ResourceId)
             .OrderBy(bt => bt.StartTime)
          .ToListAsync(cancellationToken);

        return _mapper.Map<List<BlockedTimeDto>>(blockedTimes);
    }
}
