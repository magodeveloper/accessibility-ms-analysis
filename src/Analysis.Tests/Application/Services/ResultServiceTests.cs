using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Analysis.Application.Services.Result;
using Analysis.Application.Dtos;
using Analysis.Infrastructure.Data;
using Analysis.Domain.Entities;

namespace Analysis.Tests.Application.Services;

public class ResultServiceTests : IDisposable
{
    private readonly AnalysisDbContext _context;
    private readonly ResultService _service;

    public ResultServiceTests()
    {
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AnalysisDbContext(options);
        _service = new ResultService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
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
    public async Task GetAllAsync_WithData_ShouldReturnAllResults()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnResult()
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
    public async Task GetByAnalysisIdAsync_WithValidAnalysisId_ShouldReturnMatchingResults()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByAnalysisIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().AnalysisId.Should().Be(1);
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_WithInvalidAnalysisId_ShouldReturnEmptyList()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByAnalysisIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByLevelAsync_WithValidLevel_ShouldReturnMatchingResults()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByLevelAsync("violation");

        // Assert
        result.Should().HaveCount(1);
        result.First().Level.Should().Be("violation");
    }

    [Fact]
    public async Task GetBySeverityAsync_WithValidSeverity_ShouldReturnMatchingResults()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetBySeverityAsync("high");

        // Assert
        result.Should().HaveCount(1);
        result.First().Severity.Should().Be("high");
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateResult()
    {
        // Arrange
        var dto = new ResultCreateDto(
            AnalysisId: 1,
            WcagCriterionId: 1,
            WcagCriterion: "1.1.1",
            Level: "violation",
            Severity: "high",
            Description: "Test description"
        );

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.AnalysisId.Should().Be(dto.AnalysisId);
        result.Description.Should().Be(dto.Description);

        var savedEntity = await _context.Results.FindAsync(result.Id);
        savedEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteResult()
    {
        // Arrange
        await SeedTestData();

        // Act
        await _service.DeleteAsync(1);

        // Assert
        var deletedEntity = await _context.Results.FindAsync(1);
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
    public async Task DeleteAllAsync_ShouldDeleteAllResults()
    {
        // Arrange
        await SeedTestData();

        // Act
        await _service.DeleteAllAsync();

        // Assert
        var count = await _context.Results.CountAsync();
        count.Should().Be(0);
    }

    private async Task SeedTestData()
    {
        var results = new[]
        {
            new Result
            {
                Id = 1,
                AnalysisId = 1,
                WcagCriterionId = 1,
                WcagCriterion = "1.1.1",
                Level = ResultLevel.violation,
                Severity = Severity.high,
                Description = "Test violation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Analysis = null!, // Required navigation property - set to null! for test
                Errors = new List<Error>() // Required collection
            },
            new Result
            {
                Id = 2,
                AnalysisId = 2,
                WcagCriterionId = 2,
                WcagCriterion = "1.2.1",
                Level = ResultLevel.recommendation,
                Severity = Severity.medium,
                Description = "Test recommendation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Analysis = null!, // Required navigation property - set to null! for test
                Errors = new List<Error>() // Required collection
            }
        };

        _context.Results.AddRange(results);
        await _context.SaveChangesAsync();
    }
}