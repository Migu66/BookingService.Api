using AutoMapper;
using BookingService.Api.Core.Application.Common.Mappings;
using BookingService.Api.Core.Application.Features.Resources.Commands;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.Api.Tests.Resources.Commands;

public class UpdateResourceCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly UpdateResourceCommandHandler _handler;

    public UpdateResourceCommandHandlerTests()
    {
        // Configurar base de datos en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Configurar AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Crear handler
        _handler = new UpdateResourceCommandHandler(_context, _mapper);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesResourceAndReturnsDto()
    {
        // Arrange
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Original Name",
            Description = "Original Description",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Resources.AddAsync(existingResource);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateResourceRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsActive = false
        };

        var command = new UpdateResourceCommand(1, updateRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonExistentResource_ThrowsKeyNotFoundException()
    {
        // Arrange
        var updateRequest = new UpdateResourceRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsActive = true
        };

        var command = new UpdateResourceCommand(999, updateRequest);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Resource with ID 999 not found");
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsUpdatedAt()
    {
        // Arrange
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Original Name",
            Description = "Original Description",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = null
        };

        await _context.Resources.AddAsync(existingResource);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateResourceRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsActive = true
        };

        var command = new UpdateResourceCommand(1, updateRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedResource = await _context.Resources.FindAsync(1);
        updatedResource!.UpdatedAt.Should().NotBeNull();
        updatedResource.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidRequest_PreservesCreatedAt()
    {
        // Arrange
        var originalCreatedAt = DateTime.UtcNow.AddDays(-5);
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Original Name",
            Description = "Original Description",
            IsActive = true,
            CreatedAt = originalCreatedAt
        };

        await _context.Resources.AddAsync(existingResource);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateResourceRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsActive = true
        };

        var command = new UpdateResourceCommand(1, updateRequest);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedResource = await _context.Resources.FindAsync(1);
        updatedResource!.CreatedAt.Should().BeCloseTo(originalCreatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_ActivateInactiveResource_UpdatesIsActive()
    {
        // Arrange
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Inactive Resource",
            Description = "Description",
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Resources.AddAsync(existingResource);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateResourceRequest
        {
            Name = "Inactive Resource",
            Description = "Description",
            IsActive = true
        };

        var command = new UpdateResourceCommand(1, updateRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsActive.Should().BeTrue();

        var updatedResource = await _context.Resources.FindAsync(1);
        updatedResource!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OnlyUpdateName_PreservesOtherFields()
    {
        // Arrange
        var existingResource = new Resource
        {
            Id = 1,
            Name = "Original Name",
            Description = "Original Description",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Resources.AddAsync(existingResource);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateResourceRequest
        {
            Name = "New Name Only",
            Description = "Original Description",
            IsActive = true
        };

        var command = new UpdateResourceCommand(1, updateRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("New Name Only");
        result.Description.Should().Be("Original Description");
        result.IsActive.Should().BeTrue();
    }
}
