using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Application.Features.Reservations.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BookingService.Api.Tests.Reservations.Validators;

public class CreateReservationRequestValidatorTests
{
    private readonly CreateReservationRequestValidator _validator;

    public CreateReservationRequestValidatorTests()
    {
        _validator = new CreateReservationRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ResourceIdZero_ShouldHaveError()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = 0,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ResourceId)
            .WithErrorMessage("ResourceId must be greater than 0");
    }

    [Fact]
    public void Validate_NegativeResourceId_ShouldHaveError()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = -1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ResourceId);
    }

    [Fact]
    public void Validate_StartTimeInPast_ShouldHaveError()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("StartTime must be in the future");
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_ShouldHaveError()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime must be after StartTime");
    }

    [Fact]
    public void Validate_DurationLessThan30Minutes_ShouldHaveError()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(1).AddMinutes(15)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Reservation duration must be at least 30 minutes");
    }

    [Fact]
    public void Validate_DurationExceeds4Hours_ShouldHaveError()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(6)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Reservation duration must not exceed 4 hours");
    }

    [Fact]
    public void Validate_DurationExactly30Minutes_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateReservationRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(1).AddMinutes(30)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_DurationExactly4Hours_ShouldNotHaveError()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(1);
        var request = new CreateReservationRequest
        {
            ResourceId = 1,
            StartTime = startTime,
            EndTime = startTime.AddHours(4)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
