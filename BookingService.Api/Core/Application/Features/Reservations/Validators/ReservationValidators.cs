using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using FluentValidation;

namespace BookingService.Api.Core.Application.Features.Reservations.Validators;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.ResourceId)
            .GreaterThan(0).WithMessage("ResourceId must be greater than 0");

        RuleFor(x => x.StartTime)
        .NotEmpty().WithMessage("StartTime is required")
          .GreaterThan(DateTime.UtcNow).WithMessage("StartTime must be in the future");

        RuleFor(x => x.EndTime)
        .NotEmpty().WithMessage("EndTime is required")
                  .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime");

        RuleFor(x => x)
         .Must(x => x.EndTime - x.StartTime >= TimeSpan.FromMinutes(30))
          .WithMessage("Reservation duration must be at least 30 minutes")
       .Must(x => x.EndTime - x.StartTime <= TimeSpan.FromHours(4))
                  .WithMessage("Reservation duration must not exceed 4 hours");
    }
}

public class AvailabilityRequestValidator : AbstractValidator<AvailabilityRequest>
{
    public AvailabilityRequestValidator()
    {
        RuleFor(x => x.ResourceId)
     .GreaterThan(0).WithMessage("ResourceId must be greater than 0");

        RuleFor(x => x.StartTime)
     .NotEmpty().WithMessage("StartTime is required");

        RuleFor(x => x.EndTime)
  .NotEmpty().WithMessage("EndTime is required")
       .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime");
    }
}
