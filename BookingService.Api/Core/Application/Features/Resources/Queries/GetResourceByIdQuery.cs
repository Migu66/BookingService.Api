using AutoMapper;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Resources.Queries;

public record GetResourceByIdQuery(int Id) : IRequest<ResourceDto>;

public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, ResourceDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetResourceByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResourceDto> Handle(GetResourceByIdQuery query, CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (resource == null)
        {
            throw new KeyNotFoundException($"Resource with ID {query.Id} not found");
        }

        return _mapper.Map<ResourceDto>(resource);
    }
}
