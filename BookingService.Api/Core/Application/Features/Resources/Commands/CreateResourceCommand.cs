using AutoMapper;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Infrastructure.Data;
using MediatR;

namespace BookingService.Api.Core.Application.Features.Resources.Commands;

public record CreateResourceCommand(CreateResourceRequest Request) : IRequest<ResourceDto>;

public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, ResourceDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateResourceCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResourceDto> Handle(CreateResourceCommand command, CancellationToken cancellationToken)
    {
        var resource = _mapper.Map<Resource>(command.Request);
        resource.CreatedAt = DateTime.UtcNow;
        resource.IsActive = true;

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ResourceDto>(resource);
    }
}
