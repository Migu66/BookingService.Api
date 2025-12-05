using BookingService.Api.Core.Application.Features.Reservations.Commands;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.Api.Tests.Reservations.Commands;

public class CancelReservationCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CancelReservationCommandHandler _handler;

    public CancelReservationCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new CancelReservationCommandHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<User> CreateTestUser(string name = "Test User")
    {
        var user = new User
        {
            Name = name,
            Email = $"{name.Replace(" ", "")}@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Resource> CreateTestResource()
    {
        var resource = new Resource
        {
            Name = "Conference Room",
            Description = "Test resource",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    private async Task<Reservation> CreateTestReservation(int userId, int resourceId, ReservationStatus status = ReservationStatus.Active)
    {
        var reservation = new Reservation
        {
            UserId = userId,
            ResourceId = resourceId,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    [Fact]
    public async Task Handle_OwnerCancelsOwnReservation_ReturnsTrue()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();
        var reservation = await CreateTestReservation(user.Id, resource.Id);

        var command = new CancelReservationCommand(reservation.Id, user.Id, IsAdmin: false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
        updatedReservation!.Status.Should().Be(ReservationStatus.Cancelled);
        updatedReservation.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AdminCancelsAnyReservation_ReturnsTrue()
    {
        // Arrange
        var user = await CreateTestUser();
        var admin = await CreateTestUser("Admin User");
        var resource = await CreateTestResource();
        var reservation = await CreateTestReservation(user.Id, resource.Id);

        var command = new CancelReservationCommand(reservation.Id, admin.Id, IsAdmin: true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
        updatedReservation!.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_NonOwnerNonAdminCancels_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var owner = await CreateTestUser("Owner");
        var otherUser = await CreateTestUser("Other User");
        var resource = await CreateTestResource();
        var reservation = await CreateTestReservation(owner.Id, resource.Id);

        var command = new CancelReservationCommand(reservation.Id, otherUser.Id, IsAdmin: false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*own reservations*");
    }

    [Fact]
    public async Task Handle_ReservationNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var user = await CreateTestUser();

        var command = new CancelReservationCommand(ReservationId: 999, user.Id, IsAdmin: false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_AlreadyCancelledReservation_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();
        var reservation = await CreateTestReservation(user.Id, resource.Id, ReservationStatus.Cancelled);

        var command = new CancelReservationCommand(reservation.Id, user.Id, IsAdmin: false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot cancel*");
    }

    [Fact]
    public async Task Handle_CompletedReservation_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();
        var reservation = await CreateTestReservation(user.Id, resource.Id, ReservationStatus.Completed);

        var command = new CancelReservationCommand(reservation.Id, user.Id, IsAdmin: false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot cancel*");
    }

    [Fact]
    public async Task Handle_CancelReservation_SetsUpdatedAtToCurrentTime()
    {
        // Arrange
        var user = await CreateTestUser();
        var resource = await CreateTestResource();
        var reservation = await CreateTestReservation(user.Id, resource.Id);

        var command = new CancelReservationCommand(reservation.Id, user.Id, IsAdmin: false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
        updatedReservation!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
