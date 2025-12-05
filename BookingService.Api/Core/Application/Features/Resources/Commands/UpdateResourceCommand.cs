using AutoMapper;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Resources.Commands;

public record UpdateResourceCommand(int Id, UpdateResourceRequest Request) : IRequest<ResourceDto>;

public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, ResourceDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateResourceCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResourceDto> Handle(UpdateResourceCommand command, CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
                  .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (resource == null)
        {
            throw new KeyNotFoundException($"Resource with ID {command.Id} not found");
        }

        _mapper.Map(command.Request, resource);
        resource.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ResourceDto>(resource);
    }
}
