using AutoMapper;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Reservations.Queries;

public record GetMyReservationsQuery(int UserId) : IRequest<List<ReservationDto>>;

public class GetMyReservationsQueryHandler : IRequestHandler<GetMyReservationsQuery, List<ReservationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMyReservationsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ReservationDto>> Handle(GetMyReservationsQuery query, CancellationToken cancellationToken)
    {
        var reservations = await _context.Reservations
         .Include(r => r.User)
         .Include(r => r.Resource)
                .Where(r => r.UserId == query.UserId)
       .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

        return _mapper.Map<List<ReservationDto>>(reservations);
    }
}
