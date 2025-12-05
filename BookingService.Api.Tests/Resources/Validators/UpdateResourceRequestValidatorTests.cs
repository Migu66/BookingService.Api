using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Core.Application.Features.Resources.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace BookingService.Api.Tests.Resources.Validators;

public class UpdateResourceRequestValidatorTests
{
    private readonly UpdateResourceRequestValidator _validator;

    public UpdateResourceRequestValidatorTests()
    {
        _validator = new UpdateResourceRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new UpdateResourceRequest
        {
            Name = "Updated Conference Room",
            Description = "Updated room with new equipment",
            IsActive = true
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
        var request = new UpdateResourceRequest
        {
            Name = "",
            Description = "Valid description",
            IsActive = true
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
        var request = new UpdateResourceRequest
        {
            Name = new string('a', 101), // 101 caracteres
            Description = "Valid description",
            IsActive = true
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
        var request = new UpdateResourceRequest
        {
            Name = new string('a', 100), // Exactamente 100 caracteres
            Description = "Valid description",
            IsActive = true
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
        var request = new UpdateResourceRequest
        {
            Name = "Valid Name",
            Description = "",
            IsActive = true
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
        var request = new UpdateResourceRequest
        {
            Name = "Valid Name",
            Description = new string('a', 501), // 501 caracteres
            IsActive = true
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
        var request = new UpdateResourceRequest
        {
            Name = "Valid Name",
            Description = new string('a', 500), // Exactamente 500 caracteres
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region IsActive Validation Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_IsActiveValues_ShouldNotHaveErrors(bool isActive)
    {
        // Arrange
        var request = new UpdateResourceRequest
        {
            Name = "Valid Name",
            Description = "Valid description",
            IsActive = isActive
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public void Validate_EmptyNameAndLongDescription_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = new UpdateResourceRequest
        {
            Name = "",
            Description = new string('a', 501),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    #endregion
}
