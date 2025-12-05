using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Application.Features.Reservations.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BookingService.Api.Tests.Reservations.Validators;

public class AvailabilityRequestValidatorTests
{
    private readonly AvailabilityRequestValidator _validator;

    public AvailabilityRequestValidatorTests()
    {
        _validator = new AvailabilityRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new AvailabilityRequest
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
        var request = new AvailabilityRequest
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
        var request = new AvailabilityRequest
        {
            ResourceId = -5,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ResourceId);
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_ShouldHaveError()
    {
        // Arrange
        var request = new AvailabilityRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime must be after StartTime");
    }

    [Fact]
    public void Validate_EndTimeEqualsStartTime_ShouldHaveError()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(1);
        var request = new AvailabilityRequest
        {
            ResourceId = 1,
            StartTime = startTime,
            EndTime = startTime
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime must be after StartTime");
    }

    [Fact]
    public void Validate_PastStartTime_ShouldNotHaveError()
    {
        // Arrange - AvailabilityRequest allows past times (for checking historical availability)
        var request = new AvailabilityRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Validate_DefaultStartTime_ShouldHaveError()
    {
        // Arrange
        var request = new AvailabilityRequest
        {
            ResourceId = 1,
            StartTime = default,
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("StartTime is required");
    }

    [Fact]
    public void Validate_DefaultEndTime_ShouldHaveError()
    {
        // Arrange
        var request = new AvailabilityRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = default
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime is required");
    }
}
