using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Application.Features.Auth.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace BookingService.Api.Tests.Auth.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveError()
    {
        // Arrange
        var request = new LoginRequest
        {
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
        var request = new LoginRequest
        {
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
    public void Validate_EmptyPassword_ShouldHaveError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("user.name@domain.com")]
    [InlineData("user+tag@domain.co.uk")]
    public void Validate_ValidEmailFormats_ShouldNotHaveErrors(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void Validate_InvalidEmailFormats_ShouldHaveErrors(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
