using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Application.Features.Auth.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace BookingService.Api.Tests.Auth.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password123"
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
        var request = new RegisterRequest
        {
            Name = "",
            Email = "john@example.com",
            Password = "password123"
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
        var request = new RegisterRequest
        {
            Name = new string('a', 101), // 101 caracteres
            Email = "john@example.com",
            Password = "password123"
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
        var request = new RegisterRequest
        {
            Name = new string('a', 100), // Exactamente 100 caracteres
            Email = "john@example.com",
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "",
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "invalid-email",
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveError()
    {
        // Arrange
        var longEmail = new string('a', 90) + "@example.com"; // Más de 100 caracteres
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = longEmail,
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 100 characters");
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public void Validate_EmptyPassword_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Validate_PasswordTooShort_ShouldHaveError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "12345" // Solo 5 caracteres
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters");
    }

    [Fact]
    public void Validate_PasswordAtMinLength_ShouldNotHaveError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "123456" // Exactamente 6 caracteres
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public void Validate_AllFieldsEmpty_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "",
            Email = "",
            Password = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
