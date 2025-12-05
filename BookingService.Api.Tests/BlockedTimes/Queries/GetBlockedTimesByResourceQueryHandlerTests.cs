using AutoMapper;
using BookingService.Api.Core.Application.Common.Mappings;
using BookingService.Api.Core.Application.Features.BlockedTimes.Queries;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.Api.Tests.BlockedTimes.Queries;

public class GetBlockedTimesByResourceQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetBlockedTimesByResourceQueryHandler _handler;

    public GetBlockedTimesByResourceQueryHandlerTests()
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

        _handler = new GetBlockedTimesByResourceQueryHandler(_context, _mapper);
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

    private async Task<BlockedTime> CreateTestBlockedTime(int resourceId, DateTime startTime, DateTime endTime, string reason = "Maintenance")
    {
        var blockedTime = new BlockedTime
        {
            ResourceId = resourceId,
            StartTime = startTime,
            EndTime = endTime,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
        _context.BlockedTimes.Add(blockedTime);
        await _context.SaveChangesAsync();
        return blockedTime;
    }

    [Fact]
    public async Task Handle_ResourceWithBlockedTimes_ReturnsBlockedTimesList()
    {
        // Arrange
        var resource = await CreateTestResource();
        await CreateTestBlockedTime(resource.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), "First block");
        await CreateTestBlockedTime(resource.Id, DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(4), "Second block");

        var query = new GetBlockedTimesByResourceQuery(resource.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ResourceWithNoBlockedTimes_ReturnsEmptyList()
    {
        // Arrange
        var resource = await CreateTestResource();

        var query = new GetBlockedTimesByResourceQuery(resource.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentResource_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetBlockedTimesByResourceQuery(999);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleResources_ReturnsOnlyRequestedResourceBlockedTimes()
    {
        // Arrange
        var resource1 = await CreateTestResource("Resource 1");
        var resource2 = await CreateTestResource("Resource 2");

        await CreateTestBlockedTime(resource1.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), "Block for resource 1");
        await CreateTestBlockedTime(resource2.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), "Block for resource 2");
        await CreateTestBlockedTime(resource2.Id, DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(4), "Another block for resource 2");

        var query = new GetBlockedTimesByResourceQuery(resource2.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(bt => bt.ResourceId == resource2.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_BlockedTimes_ReturnsOrderedByStartTime()
    {
        // Arrange
        var resource = await CreateTestResource();

        var laterTime = DateTime.UtcNow.AddHours(5);
        var earlierTime = DateTime.UtcNow.AddHours(1);
        var middleTime = DateTime.UtcNow.AddHours(3);

        await CreateTestBlockedTime(resource.Id, laterTime, laterTime.AddHours(1), "Later");
        await CreateTestBlockedTime(resource.Id, earlierTime, earlierTime.AddHours(1), "Earlier");
        await CreateTestBlockedTime(resource.Id, middleTime, middleTime.AddHours(1), "Middle");

        var query = new GetBlockedTimesByResourceQuery(resource.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].Reason.Should().Be("Earlier");
        result[1].Reason.Should().Be("Middle");
        result[2].Reason.Should().Be("Later");
    }

    [Fact]
    public async Task Handle_BlockedTime_ReturnsCorrectDtoProperties()
    {
        // Arrange
        var resource = await CreateTestResource("Main Room");
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = DateTime.UtcNow.AddHours(3);

        await CreateTestBlockedTime(resource.Id, startTime, endTime, "Scheduled maintenance");

        var query = new GetBlockedTimesByResourceQuery(resource.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var blockedTimeDto = result[0];
        blockedTimeDto.ResourceId.Should().Be(resource.Id);
        blockedTimeDto.ResourceName.Should().Be("Main Room");
        blockedTimeDto.Reason.Should().Be("Scheduled maintenance");
        blockedTimeDto.StartTime.Should().BeCloseTo(startTime, TimeSpan.FromSeconds(1));
        blockedTimeDto.EndTime.Should().BeCloseTo(endTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_BlockedTime_ReturnsIdGreaterThanZero()
    {
        // Arrange
        var resource = await CreateTestResource();
        await CreateTestBlockedTime(resource.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var query = new GetBlockedTimesByResourceQuery(resource.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().BeGreaterThan(0);
    }
}
