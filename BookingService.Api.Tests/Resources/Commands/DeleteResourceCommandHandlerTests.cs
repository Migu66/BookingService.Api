using BookingService.Api.Core.Application.Features.Resources.Commands;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.Api.Tests.Resources.Commands;

public class DeleteResourceCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DeleteResourceCommandHandler _handler;

    public DeleteResourceCommandHandlerTests()
    {
        // Configurar base de datos en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Crear handler
        _handler = new DeleteResourceCommandHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingResource_DeletesAndReturnsTrue()
    {
        // Arrange
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Resource To Delete",
            Description = "This resource will be deleted",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Resources.AddAsync(existingResource);
        await _context.SaveChangesAsync();

        var command = new DeleteResourceCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var deletedResource = await _context.Resources.FindAsync(1);
        deletedResource.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentResource_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new DeleteResourceCommand(999);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Resource with ID 999 not found");
    }

    [Fact]
    public async Task Handle_DeletesOnlySpecifiedResource_LeavesOthersIntact()
    {
        // Arrange
        var resource1 = new Resource
        {
            Id = 1,
            Name = "Resource 1",
            Description = "First resource",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var resource2 = new Resource
        {
            Id = 2,
            Name = "Resource 2",
            Description = "Second resource",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Resources.AddRangeAsync(resource1, resource2);
        await _context.SaveChangesAsync();

        var command = new DeleteResourceCommand(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingResources = await _context.Resources.ToListAsync();
        remainingResources.Should().HaveCount(1);
        remainingResources[0].Id.Should().Be(2);
        remainingResources[0].Name.Should().Be("Resource 2");
    }

    [Fact]
    public async Task Handle_InactiveResource_DeletesSuccessfully()
    {
        // Arrange
        var inactiveResource = new Resource
        {
            Id = 1,
            Name = "Inactive Resource",
            Description = "This is an inactive resource",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Resources.AddAsync(inactiveResource);
        await _context.SaveChangesAsync();

        var command = new DeleteResourceCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var deletedResource = await _context.Resources.FindAsync(1);
        deletedResource.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeleteTwice_SecondAttemptThrowsKeyNotFoundException()
    {
        // Arrange
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Resource To Delete Twice",
            Description = "Description",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Resources.AddAsync(existingResource);
        await _context.SaveChangesAsync();

        var command = new DeleteResourceCommand(1);

        // Act - First delete
        await _handler.Handle(command, CancellationToken.None);

        // Act & Assert - Second delete attempt
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Resource with ID 1 not found");
    }

    [Fact]
    public async Task Handle_ResourceWithUpdatedAt_DeletesSuccessfully()
    {
        // Arrange
        var resourceWithUpdates = new Resource
        {
            Id = 1,
            Name = "Updated Resource",
            Description = "This resource was previously updated",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Resources.AddAsync(resourceWithUpdates);
        await _context.SaveChangesAsync();

        var command = new DeleteResourceCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var count = await _context.Resources.CountAsync();
        count.Should().Be(0);
    }
}
