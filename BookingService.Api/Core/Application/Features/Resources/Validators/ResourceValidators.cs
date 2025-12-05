using BookingService.Api.Core.Application.Features.Resources.DTOs;
using FluentValidation;

namespace BookingService.Api.Core.Application.Features.Resources.Validators;

public class CreateResourceRequestValidator : AbstractValidator<CreateResourceRequest>
{
    public CreateResourceRequestValidator()
    {
        RuleFor(x => x.Name)
           .NotEmpty().WithMessage("Name is required")
             .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
           .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}

public class UpdateResourceRequestValidator : AbstractValidator<UpdateResourceRequest>
{
    public UpdateResourceRequestValidator()
    {
        RuleFor(x => x.Name)
           .NotEmpty().WithMessage("Name is required")
                    .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
         .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
