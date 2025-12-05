using AutoMapper;
using BookingService.Api.Core.Application.Common.Mappings;
using BookingService.Api.Core.Application.Features.BlockedTimes.Commands;
using BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.Api.Tests.BlockedTimes.Commands;

public class CreateBlockedTimeCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CreateBlockedTimeCommandHandler _handler;

    public CreateBlockedTimeCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateBlockedTimeCommandHandler(_context, _mapper);
    }

    public void Dispose()
    {
        _context.Dispose();
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
    public async Task Handle_ValidRequest_CreatesBlockedTimeAndReturnsDto()
    {
        // Arrange
        var resource = await CreateTestResource();

        var request = new CreateBlockedTimeRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Maintenance"
        };

        var command = new CreateBlockedTimeCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ResourceId.Should().Be(resource.Id);
        result.Reason.Should().Be("Maintenance");

        var savedBlockedTime = await _context.BlockedTimes.FirstOrDefaultAsync();
        savedBlockedTime.Should().NotBeNull();
        savedBlockedTime!.Reason.Should().Be("Maintenance");
    }

    [Fact]
    public async Task Handle_ResourceNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CreateBlockedTimeRequest
        {
            ResourceId = 999,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Maintenance"
        };

        var command = new CreateBlockedTimeCommand(request);

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
        var resource = await CreateTestResource(isActive: false);

        var request = new CreateBlockedTimeRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Maintenance"
        };

        var command = new CreateBlockedTimeCommand(request);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsCreatedAtToCurrentTime()
    {
        // Arrange
        var resource = await CreateTestResource();

        var request = new CreateBlockedTimeRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Scheduled maintenance"
        };

        var command = new CreateBlockedTimeCommand(request);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedBlockedTime = await _context.BlockedTimes.FirstOrDefaultAsync();
        savedBlockedTime.Should().NotBeNull();
        savedBlockedTime!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCorrectId()
    {
        // Arrange
        var resource = await CreateTestResource();

        var request = new CreateBlockedTimeRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Testing ID assignment"
        };

        var command = new CreateBlockedTimeCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().BeGreaterThan(0);

        var savedBlockedTime = await _context.BlockedTimes.FindAsync(result.Id);
        savedBlockedTime.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_MultipleBlockedTimes_CreatesAllSuccessfully()
    {
        // Arrange
        var resource = await CreateTestResource();

        var request1 = new CreateBlockedTimeRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Reason = "First blocked time"
        };

        var request2 = new CreateBlockedTimeRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(4),
            Reason = "Second blocked time"
        };

        // Act
        var result1 = await _handler.Handle(new CreateBlockedTimeCommand(request1), CancellationToken.None);
        var result2 = await _handler.Handle(new CreateBlockedTimeCommand(request2), CancellationToken.None);

        // Assert
        result1.Id.Should().NotBe(result2.Id);

        var count = await _context.BlockedTimes.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsResourceName()
    {
        // Arrange
        var resource = await CreateTestResource("Main Conference Room");

        var request = new CreateBlockedTimeRequest
        {
            ResourceId = resource.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Reason = "Renovation"
        };

        var command = new CreateBlockedTimeCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ResourceName.Should().Be("Main Conference Room");
    }
}
