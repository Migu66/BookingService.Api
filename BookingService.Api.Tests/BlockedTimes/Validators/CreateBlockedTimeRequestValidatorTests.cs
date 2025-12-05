using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using BookingService.Api.Core.Application.Features.BlockedTimes.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BookingService.Api.Tests.BlockedTimes.Validators;

public class CreateBlockedTimeRequestValidatorTests
{
    private readonly CreateBlockedTimeRequestValidator _validator;

    public CreateBlockedTimeRequestValidatorTests()
    {
        _validator = new CreateBlockedTimeRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Scheduled maintenance"
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
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 0,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Maintenance"
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
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = -1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Maintenance"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ResourceId);
    }

    [Fact]
    public void Validate_DefaultStartTime_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = default,
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Maintenance"
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
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = default,
            Reason = "Maintenance"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime is required");
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(1),
            Reason = "Maintenance"
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
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = startTime,
            EndTime = startTime,
            Reason = "Maintenance"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime must be after StartTime");
    }

    [Fact]
    public void Validate_EmptyReason_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason is required");
    }

    [Fact]
    public void Validate_ReasonExceeds200Characters_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = new string('a', 201)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason must not exceed 200 characters");
    }

    [Fact]
    public void Validate_ReasonExactly200Characters_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = new string('a', 200)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_WhitespaceOnlyReason_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "   "
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
