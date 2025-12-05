using AutoMapper;
using BookingService.Api.Core.Application.Common.Mappings;
using BookingService.Api.Core.Application.Features.Auth.Commands;
using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using BookingService.Api.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BookingService.Api.Tests.Auth.Commands;

public class RegisterCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly IMapper _mapper;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        // Configurar base de datos en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Configurar mocks
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();

        // Configurar AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Crear handler
        _handler = new RegisterCommandHandler(
            _context,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _mapper);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserAndReturnsAuthResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        _passwordHasherMock
            .Setup(x => x.HashPassword("password123"))
            .Returns("hashedPassword123");

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<int>(), "newuser@example.com", "User"))
            .Returns("new-user-jwt-token");

        var command = new RegisterCommand(registerRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
        result.Name.Should().Be("New User");
        result.Token.Should().Be("new-user-jwt-token");

        // Verificar que el usuario se guardó en la base de datos
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.Name.Should().Be("New User");
        savedUser.PasswordHash.Should().Be("hashedPassword123");
        savedUser.Role.Should().Be(UserRole.User);
        savedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Name = "Existing User",
            Email = "existing@example.com",
            PasswordHash = "hashedPassword",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(existingUser);
        await _context.SaveChangesAsync();

        var registerRequest = new RegisterRequest
        {
            Name = "New User",
            Email = "existing@example.com", // Email duplicado
            Password = "password123"
        };

        var command = new RegisterCommand(registerRequest);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists");
    }

    [Fact]
    public async Task Handle_NewUser_SetsCorrectDefaultValues()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Name = "Default Values User",
            Email = "defaults@example.com",
            Password = "password123"
        };

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("token");

        var command = new RegisterCommand(registerRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "defaults@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.Role.Should().Be(UserRole.User); // Rol por defecto es User
        savedUser.IsActive.Should().BeTrue(); // Usuario activo por defecto
        savedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsPasswordHasher()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "mySecurePassword"
        };

        _passwordHasherMock
            .Setup(x => x.HashPassword("mySecurePassword"))
            .Returns("hashedPassword");

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("token");

        var command = new RegisterCommand(registerRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasherMock.Verify(x => x.HashPassword("mySecurePassword"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsTokenService()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Name = "Test User",
            Email = "tokentest@example.com",
            Password = "password123"
        };

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<int>(), "tokentest@example.com", "User"))
            .Returns("generated-token");

        var command = new RegisterCommand(registerRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _tokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<int>(), "tokentest@example.com", "User"),
            Times.Once);
    }
}
