using BookingService.Api.Core.Application.Features.BlockedTimes.Commands;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.Api.Tests.BlockedTimes.Commands;

public class DeleteBlockedTimeCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DeleteBlockedTimeCommandHandler _handler;

    public DeleteBlockedTimeCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new DeleteBlockedTimeCommandHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<Resource> CreateTestResource(string name = "Conference Room")
    {
        var resource = new Resource
        {
            Name = name,
            Description = "Test resource",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    private async Task<BlockedTime> CreateTestBlockedTime(int resourceId, string reason = "Maintenance")
    {
        var blockedTime = new BlockedTime
        {
            ResourceId = resourceId,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
        _context.BlockedTimes.Add(blockedTime);
        await _context.SaveChangesAsync();
        return blockedTime;
    }

    [Fact]
    public async Task Handle_ExistingBlockedTime_DeletesAndReturnsTrue()
    {
        // Arrange
        var resource = await CreateTestResource();
        var blockedTime = await CreateTestBlockedTime(resource.Id);

        var command = new DeleteBlockedTimeCommand(blockedTime.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var deletedBlockedTime = await _context.BlockedTimes.FindAsync(blockedTime.Id);
        deletedBlockedTime.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentBlockedTime_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new DeleteBlockedTimeCommand(999);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_DeletesOnlySpecifiedBlockedTime_LeavesOthersIntact()
    {
        // Arrange
        var resource = await CreateTestResource();
        var blockedTime1 = await CreateTestBlockedTime(resource.Id, "First blocked time");
        var blockedTime2 = await CreateTestBlockedTime(resource.Id, "Second blocked time");

        var command = new DeleteBlockedTimeCommand(blockedTime1.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingBlockedTimes = await _context.BlockedTimes.ToListAsync();
        remainingBlockedTimes.Should().HaveCount(1);
        remainingBlockedTimes[0].Id.Should().Be(blockedTime2.Id);
        remainingBlockedTimes[0].Reason.Should().Be("Second blocked time");
    }

    [Fact]
    public async Task Handle_DeleteTwice_SecondAttemptThrowsKeyNotFoundException()
    {
        // Arrange
        var resource = await CreateTestResource();
        var blockedTime = await CreateTestBlockedTime(resource.Id);

        var command = new DeleteBlockedTimeCommand(blockedTime.Id);

        // Act - First delete
        await _handler.Handle(command, CancellationToken.None);

        // Act & Assert - Second delete attempt
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_BlockedTimeFromDifferentResource_DeletesSuccessfully()
    {
        // Arrange
        var resource1 = await CreateTestResource("Resource 1");
        var resource2 = await CreateTestResource("Resource 2");
        var blockedTime1 = await CreateTestBlockedTime(resource1.Id, "Blocked for resource 1");
        var blockedTime2 = await CreateTestBlockedTime(resource2.Id, "Blocked for resource 2");

        var command = new DeleteBlockedTimeCommand(blockedTime1.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var remainingBlockedTimes = await _context.BlockedTimes.ToListAsync();
        remainingBlockedTimes.Should().HaveCount(1);
        remainingBlockedTimes[0].ResourceId.Should().Be(resource2.Id);
    }

    [Fact]
    public async Task Handle_DeleteBlockedTime_ResourceRemains()
    {
        // Arrange
        var resource = await CreateTestResource();
        var blockedTime = await CreateTestBlockedTime(resource.Id);

        var command = new DeleteBlockedTimeCommand(blockedTime.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var existingResource = await _context.Resources.FindAsync(resource.Id);
        existingResource.Should().NotBeNull();
        existingResource!.Name.Should().Be("Conference Room");
    }
}
