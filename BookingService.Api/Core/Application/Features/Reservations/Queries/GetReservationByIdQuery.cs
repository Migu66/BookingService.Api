using AutoMapper;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Reservations.Queries;

public record GetReservationByIdQuery(int Id, int UserId, bool IsAdmin) : IRequest<ReservationDto>;

public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetReservationByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReservationDto> Handle(GetReservationByIdQuery query, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
       .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID {query.Id} not found");
        }

        // Check authorization: user can only view their own reservations, unless they're admin
        if (!query.IsAdmin && reservation.UserId != query.UserId)
        {
            throw new UnauthorizedAccessException("You can only view your own reservations");
        }

        return _mapper.Map<ReservationDto>(reservation);
    }
}
