using BookingService.Api.Core.Application.Features.Auth.Commands;
using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using BookingService.Api.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace BookingService.Api.Tests.Auth.Commands;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        // Configurar base de datos en memoria con nombre único para evitar conflictos
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Configurar mocks
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _configurationMock = new Mock<IConfiguration>();

        // Mock configuration section
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(x => x["AccessTokenExpiryMinutes"]).Returns("15");
        jwtSectionMock.Setup(x => x["RefreshTokenExpiryDays"]).Returns("7");
        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        // Crear handler
        _handler = new LoginCommandHandler(
            _context,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _configurationMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedPassword123",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("password123", "hashedPassword123"))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Email, "User"))
            .Returns("generated-jwt-token");

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken(user.Id, It.IsAny<string>()))
            .Returns(new RefreshToken
            {
                Token = "refresh-token",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            });

        var command = new LoginCommand(loginRequest, "127.0.0.1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.Name.Should().Be("Test User");
        result.AccessToken.Should().Be("generated-jwt-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        var command = new LoginCommand(loginRequest, "127.0.0.1");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedPassword123",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongPassword"
        };

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("wrongPassword", "hashedPassword123"))
            .Returns(false);

        var command = new LoginCommand(loginRequest, "127.0.0.1");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Inactive User",
            Email = "inactive@example.com",
            PasswordHash = "hashedPassword123",
            Role = UserRole.User,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "password123"
        };

        var command = new LoginCommand(loginRequest, "127.0.0.1");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_AdminUser_ReturnsAdminRole()
    {
        // Arrange
        var adminUser = new User
        {
            Id = 1,
            Name = "Admin User",
            Email = "admin@example.com",
            PasswordHash = "hashedPassword123",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "admin@example.com",
            Password = "password123"
        };

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("password123", "hashedPassword123"))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(adminUser.Id, adminUser.Email, "Admin"))
            .Returns("admin-jwt-token");

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken(adminUser.Id, It.IsAny<string>()))
            .Returns(new RefreshToken
            {
                Token = "admin-refresh-token",
                UserId = adminUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            });

        var command = new LoginCommand(loginRequest, "127.0.0.1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("Admin");
        result.AccessToken.Should().Be("admin-jwt-token");
    }
}
