using AutoMapper;
using BookingService.Api.Core.Application.Common.Mappings;
using BookingService.Api.Core.Application.Features.Reservations.Commands;
using BookingService.Api.Core.Application.Features.Reservations.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace BookingService.Api.Tests.Reservations.Commands;

public class CreateReservationCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CreateReservationCommandHandler _handler;

    public CreateReservationCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateReservationCommandHandler(_context, _mapper);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<User> CreateTestUser(string name = "Test User", string email = "test@example.com")
    {
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = "hashedpassword",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Resource> CreateTestResource(string name = "Conference Room", bool isActive = true)
    {
        var resource = new Resource
        {
            Name = name,
            Description = "Test resource",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesReservationAndReturnsDto()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();

        var request = new CreateReservationRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ResourceId.Should().Be(resource.Id);
        result.UserId.Should().Be(user.Id);
        result.Status.Should().Be("Active");

        var savedReservation = await _context.Reservations.FirstOrDefaultAsync();
        savedReservation.Should().NotBeNull();
        savedReservation!.Status.Should().Be(ReservationStatus.Active);
    }

    [Fact]
    public async Task Handle_ResourceNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var user = await CreateTestUser();

        var request = new CreateReservationRequest
        {
            ResourceId = 999,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_InactiveResource_ThrowsKeyNotFoundException()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource(isActive: false);

        var request = new CreateReservationRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_OverlappingReservation_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();

        // Create existing reservation
        var existingReservation = new Reservation
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Status = ReservationStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(existingReservation);
        await _context.SaveChangesAsync();

        // Try to create overlapping reservation
        var request = new CreateReservationRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*overlaps*");
    }

    [Fact]
    public async Task Handle_NonOverlappingReservation_CreatesSuccessfully()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();

        // Create existing reservation
        var existingReservation = new Reservation
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Status = ReservationStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(existingReservation);
        await _context.SaveChangesAsync();

        // Create non-overlapping reservation
        var request = new CreateReservationRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(4)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var count = await _context.Reservations.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_CancelledReservationSlot_AllowsNewReservation()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();

        // Create cancelled reservation
        var cancelledReservation = new Reservation
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Status = ReservationStatus.Cancelled,
            CreatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(cancelledReservation);
        await _context.SaveChangesAsync();

        // Create reservation in same slot
        var request = new CreateReservationRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_BlockedTime_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();

        // Create blocked time
        var blockedTime = new BlockedTime
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Maintenance",
            CreatedAt = DateTime.UtcNow
        };
        _context.BlockedTimes.Add(blockedTime);
        await _context.SaveChangesAsync();

        var request = new CreateReservationRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*blocked*");
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsCreatedAtToCurrentTime()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();

        var request = new CreateReservationRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        var command = new CreateReservationCommand(user.Id, request);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedReservation = await _context.Reservations.FirstOrDefaultAsync();
        savedReservation.Should().NotBeNull();
        savedReservation!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
