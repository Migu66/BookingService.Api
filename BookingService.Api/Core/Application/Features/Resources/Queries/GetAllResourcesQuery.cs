using AutoMapper;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Resources.Queries;

public record GetAllResourcesQuery : IRequest<List<ResourceDto>>;

public class GetAllResourcesQueryHandler : IRequestHandler<GetAllResourcesQuery, List<ResourceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllResourcesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ResourceDto>> Handle(GetAllResourcesQuery query, CancellationToken cancellationToken)
    {
        var resources = await _context.Resources
   .Where(r => r.IsActive)
   .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ResourceDto>>(resources);
    }
}
