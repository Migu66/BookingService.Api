using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using FluentValidation;

namespace BookingService.Api.Core.Application.Features.BlockedTimes.Validators;

public class CreateBlockedTimeRequestValidator : AbstractValidator<CreateBlockedTimeRequest>
{
    public CreateBlockedTimeRequestValidator()
    {
        RuleFor(x => x.ResourceId)
         .GreaterThan(0).WithMessage("ResourceId must be greater than 0");

        RuleFor(x => x.StartTime)
      .NotEmpty().WithMessage("StartTime is required");

        RuleFor(x => x.EndTime)
        .NotEmpty().WithMessage("EndTime is required")
          .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime");

        RuleFor(x => x.Reason)
           .NotEmpty().WithMessage("Reason is required")
     .MaximumLength(200).WithMessage("Reason must not exceed 200 characters");
    }
}
