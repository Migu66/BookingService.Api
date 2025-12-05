using AutoMapper;
using BookingService.Api.Core.Application.Common.Mappings;
using BookingService.Api.Core.Application.Features.Resources.Commands;
using BookingService.Api.Core.Application.Features.Resources.DTOs;
using BookingService.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.Api.Tests.Resources.Commands;

public class CreateResourceCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CreateResourceCommandHandler _handler;

    public CreateResourceCommandHandlerTests()
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
        _handler = new CreateResourceCommandHandler(_context, _mapper);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesResourceAndReturnsDto()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Conference Room A",
            Description = "Room with projector and video conferencing"
        };

        var command = new CreateResourceCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Conference Room A");
        result.Description.Should().Be("Room with projector and video conferencing");
        result.IsActive.Should().BeTrue();

        // Verificar que el recurso se guardó en la base de datos
        var savedResource = await _context.Resources.FirstOrDefaultAsync(r => r.Name == "Conference Room A");
        savedResource.Should().NotBeNull();
        savedResource!.Description.Should().Be("Room with projector and video conferencing");
        savedResource.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NewResource_SetsCorrectDefaultValues()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Default Values Resource",
            Description = "Testing default values"
        };

        var command = new CreateResourceCommand(request);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedResource = await _context.Resources.FirstOrDefaultAsync(r => r.Name == "Default Values Resource");
        savedResource.Should().NotBeNull();
        savedResource!.IsActive.Should().BeTrue(); // Recurso activo por defecto
        savedResource.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedResource.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCorrectId()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Resource With Id",
            Description = "Testing ID assignment"
        };

        var command = new CreateResourceCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().BeGreaterThan(0);

        var savedResource = await _context.Resources.FindAsync(result.Id);
        savedResource.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_EmptyDescription_CreatesResourceSuccessfully()
    {
        // Arrange
        var request = new CreateResourceRequest
        {
            Name = "Resource Without Description",
            Description = ""
        };

        var command = new CreateResourceCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Resource Without Description");
        result.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleResources_CreatesAllSuccessfully()
    {
        // Arrange
        var request1 = new CreateResourceRequest { Name = "Resource 1", Description = "First resource" };
        var request2 = new CreateResourceRequest { Name = "Resource 2", Description = "Second resource" };

        // Act
        var result1 = await _handler.Handle(new CreateResourceCommand(request1), CancellationToken.None);
        var result2 = await _handler.Handle(new CreateResourceCommand(request2), CancellationToken.None);

        // Assert
        result1.Id.Should().NotBe(result2.Id);

        var count = await _context.Resources.CountAsync();
        count.Should().Be(2);
    }
}
