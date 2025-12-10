using AutoMapper;
using BookingService.Api.Core.Application.Common.Models;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Reservations.Queries;

public record GetAllReservationsQuery(int PageNumber, int PageSize) : IRequest<PagedResult<ReservationDto>>;

public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, PagedResult<ReservationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllReservationsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ReservationDto>> Handle(GetAllReservationsQuery query, CancellationToken cancellationToken)
    {
        var queryable = _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);

        var reservations = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ReservationDto>
        {
            Items = _mapper.Map<List<ReservationDto>>(reservations),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
