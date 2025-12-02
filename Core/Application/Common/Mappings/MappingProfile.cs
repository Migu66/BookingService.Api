using AutoMapper;
using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Core.Domain.Common;

namespace BookingService.Api.Core.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, AuthResponse>()
       .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
       .ForMember(dest => dest.Token, opt => opt.Ignore());

        // Resource mappings
        CreateMap<Resource, ResourceDto>();
        CreateMap<CreateResourceRequest, Resource>();
        CreateMap<UpdateResourceRequest, Resource>();

        // Reservation mappings
        CreateMap<Reservation, ReservationDto>()
          .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.ResourceName, opt => opt.MapFrom(src => src.Resource.Name))
         .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

   CreateMap<CreateReservationRequest, Reservation>();

        // BlockedTime mappings
        CreateMap<BlockedTime, BlockedTimeDto>()
       .ForMember(dest => dest.ResourceName, opt => opt.MapFrom(src => src.Resource.Name));

    CreateMap<CreateBlockedTimeRequest, BlockedTime>();
    }
}
