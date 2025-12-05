using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Core.Application.Features.Resources.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace BookingService.Api.Tests.Resources.Validators;

public class CreateResourceRequestValidatorTests
{
    private readonly CreateResourceRequestValidator _validator;

    public CreateResourceRequestValidatorTests()
    {
        _validator = new CreateResourceRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Conference Room A",
            Description = "Room with projector and video conferencing"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Name Validation Tests

    [Fact]
    public void Validate_EmptyName_ShouldHaveError()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "",
            Description = "Valid description"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveError()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = new string('a', 101), // 101 caracteres
            Description = "Valid description"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_NameAtMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = new string('a', 100), // Exactamente 100 caracteres
            Description = "Valid description"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public void Validate_EmptyDescription_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Valid Name",
            Description = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Valid Name",
            Description = new string('a', 501) // 501 caracteres
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Fact]
    public void Validate_DescriptionAtMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Valid Name",
            Description = new string('a', 500) // Exactamente 500 caracteres
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public void Validate_EmptyNameAndLongDescription_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "",
            Description = new string('a', 501)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    #endregion
}
