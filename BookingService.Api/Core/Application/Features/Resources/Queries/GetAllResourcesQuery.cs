using AutoMapper;
using BookingService.Api.Core.Application.Common.Models;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Resources.Queries;

public record GetAllResourcesQuery(int PageNumber, int PageSize) : IRequest<PagedResult<ResourceDto>>;

public class GetAllResourcesQueryHandler : IRequestHandler<GetAllResourcesQuery, PagedResult<ResourceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllResourcesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ResourceDto>> Handle(GetAllResourcesQuery query, CancellationToken cancellationToken)
    {
        var queryable = _context.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name);

        var totalCount = await queryable.CountAsync(cancellationToken);

        var resources = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ResourceDto>
        {
            Items = _mapper.Map<List<ResourceDto>>(resources),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
