using Analysis.Infrastructure.Data;
using Analysis.Application.Services.Error;
using Analysis.Domain.Entities;
using Analysis.Application.Dtos;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace Analysis.Tests.UnitTests.Services;

public class ErrorServiceTests : IDisposable
{
    private readonly AnalysisDbContext _context;
    private readonly ErrorService _service;
    private bool _disposed;

    public ErrorServiceTests()
    {
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AnalysisDbContext(options);
        _service = new ErrorService(_context);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ErrorServiceTests()
    {
        Dispose(false);
    }

    [Fact]
    public async Task GetAllAsync_WithNoData_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithData_ShouldReturnAllErrors()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnError()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByResultIdAsync_WithValidResultId_ShouldReturnMatchingErrors()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByResultIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().ResultId.Should().Be(1);
    }

    [Fact]
    public async Task GetByResultIdAsync_WithInvalidResultId_ShouldReturnEmptyList()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByResultIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateError()
    {
        // Arrange
        var dto = new ErrorCreateDto(
            ResultId: 1,
            WcagCriterionId: 1,
            ErrorCode: "E001",
            Description: "Test error description",
            Location: "line 10"
        );

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.ResultId.Should().Be(dto.ResultId);
        result.Description.Should().Be(dto.Description);
        result.ErrorCode.Should().Be(dto.ErrorCode);

        var savedEntity = await _context.Errors.FindAsync(result.Id);
        savedEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteError()
    {
        // Arrange
        await SeedTestData();

        // Act
        await _service.DeleteAsync(1);

        // Assert
        var deletedEntity = await _context.Errors.FindAsync(1);
        deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldNotThrow()
    {
        // Act & Assert
        var act = async () => await _service.DeleteAsync(999);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldDeleteAllErrors()
    {
        // Arrange
        await SeedTestData();

        // Act
        await _service.DeleteAllAsync();

        // Assert
        var count = await _context.Errors.CountAsync();
        count.Should().Be(0);
    }

    private async Task SeedTestData()
    {
        var errors = new[]
        {
            new Error
            {
                Id = 1,
                ResultId = 1,
                WcagCriterionId = 1,
                ErrorCode = "E001",
                Description = "Test error 1",
                Location = "line 10",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = null! // Required navigation property - set to null! for test
            },
            new Error
            {
                Id = 2,
                ResultId = 2,
                WcagCriterionId = 2,
                ErrorCode = "E002",
                Description = "Test error 2",
                Location = "line 20",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = null! // Required navigation property - set to null! for test
            }
        };

        _context.Errors.AddRange(errors);
        await _context.SaveChangesAsync();
    }
}