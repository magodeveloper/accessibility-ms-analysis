using Moq;
using FluentAssertions;
using Analysis.Domain.Services;
using Analysis.Domain.Entities;
using Analysis.Application.Dtos;
using Microsoft.Extensions.Logging;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Analysis.Application.Services.Analysis;

namespace Analysis.Tests.Application.Services;

public class AnalysisServiceTests : IDisposable
{
    private readonly AnalysisDbContext _context;
    private readonly Mock<IUserValidationService> _mockUserValidationService;
    private readonly Mock<ILogger<AnalysisService>> _mockLogger;
    private readonly AnalysisService _service;
    private bool _disposed;

    public AnalysisServiceTests()
    {
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AnalysisDbContext(options);
        _mockUserValidationService = new Mock<IUserValidationService>();
        _mockLogger = new Mock<ILogger<AnalysisService>>();

        _service = new AnalysisService(_context, _mockUserValidationService.Object, _mockLogger.Object);

        // Setup default user validation to return true
        _ = _mockUserValidationService
            .Setup(x => x.ValidateUserExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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

    [Fact]
    public async Task GetAllAsync_WithNoData_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        _ = result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithData_ShouldReturnAllAnalyses()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        _ = result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnUserAnalyses()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByUserIdAsync(1);

        // Assert
        _ = result.Should().HaveCount(1);
        _ = result.First().UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithInvalidUserId_ShouldReturnEmptyList()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByUserIdAsync(999);

        // Assert
        _ = result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnAnalysis()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        _ = result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDateAsync_WithValidDate_ShouldReturnMatchingAnalyses()
    {
        // Arrange
        var testDate = DateTime.UtcNow.Date;
        await SeedTestDataWithSpecificDate(testDate);

        // Act
        var result = await _service.GetByDateAsync(1, testDate);

        // Assert
        _ = result.Should().HaveCount(1);
        _ = result.First().DateAnalysis.Date.Should().Be(testDate);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithValidRange_ShouldReturnMatchingAnalyses()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(-1);
        await SeedTestDataWithDateRange(startDate, endDate);

        // Act
        var result = await _service.GetByDateRangeAsync(1, startDate, endDate);

        // Assert
        _ = result.Should().HaveCount(2);
        _ = result.All(a => a.DateAnalysis >= startDate && a.DateAnalysis <= endDate).Should().BeTrue();
    }

    [Fact]
    public async Task GetByToolAsync_WithValidTool_ShouldReturnMatchingAnalyses()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByToolAsync(1, "axecore");

        // Assert
        _ = result.Should().HaveCount(1);
        _ = result.First().ToolUsed.Should().Be("axecore");
    }

    [Fact]
    public async Task GetByStatusAsync_WithValidStatus_ShouldReturnMatchingAnalyses()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _service.GetByStatusAsync(1, "pending");

        // Assert
        _ = result.Should().HaveCount(1);
        _ = result.First().Status.Should().Be("pending");
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAnalysis()
    {
        // Arrange
        var dto = CreateValidAnalysisCreateDto();

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.UserId.Should().Be(dto.UserId);
        _ = result.ContentType.Should().Be(dto.ContentType);
        _ = result.ToolUsed.Should().Be(dto.ToolUsed);

        var savedEntity = await _context.Analyses.FindAsync(result.Id);
        _ = savedEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteAnalysis()
    {
        // Arrange
        await SeedTestData();

        // Act
        await _service.DeleteAsync(1);

        // Assert
        var deletedEntity = await _context.Analyses.FindAsync(1);
        _ = deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldNotThrow()
    {
        // Act & Assert
        var act = async () => await _service.DeleteAsync(999);
        _ = await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldDeleteAllAnalyses()
    {
        // Arrange
        await SeedTestData();

        // Act
        await _service.DeleteAllAsync();

        // Assert
        var count = await _context.Analyses.CountAsync();
        _ = count.Should().Be(0);
    }

    private async Task SeedTestData()
    {
        var analyses = new[]
        {
            new Analysis.Domain.Entities.Analysis
            {
                Id = 1,
                UserId = 1,
                DateAnalysis = DateTime.UtcNow,
                ContentType = ContentType.html,
                ContentInput = "test content",
                SourceUrl = "https://test.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.pending,
                SummaryResult = "Test summary",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = [] // Required collection
            },
            new Analysis.Domain.Entities.Analysis
            {
                Id = 2,
                UserId = 2,
                DateAnalysis = DateTime.UtcNow.AddDays(-1),
                ContentType = ContentType.url,
                ContentInput = "https://example.com",
                SourceUrl = "https://example.com",
                ToolUsed = ToolUsed.equalaccess,
                Status = AnalysisStatus.success,
                SummaryResult = "Test summary 2",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.A,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = [] // Required collection
            }
        };

        _context.Analyses.AddRange(analyses);
        _ = await _context.SaveChangesAsync();
    }

    private async Task SeedTestDataWithSpecificDate(DateTime date)
    {
        var analysis = new Analysis.Domain.Entities.Analysis
        {
            Id = 1,
            UserId = 1,
            DateAnalysis = date,
            ContentType = ContentType.html,
            ContentInput = "test content",
            SourceUrl = "https://test.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.pending,
            SummaryResult = "Test summary",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = [] // Required collection
        };

        _ = _context.Analyses.Add(analysis);
        _ = await _context.SaveChangesAsync();
    }

    private async Task SeedTestDataWithDateRange(DateTime startDate, DateTime endDate)
    {
        var analyses = new[]
        {
            new Analysis.Domain.Entities.Analysis
            {
                Id = 1,
                UserId = 1,
                DateAnalysis = startDate.AddDays(1),
                ContentType = ContentType.html,
                ContentInput = "test content",
                SourceUrl = "https://test.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.pending,
                SummaryResult = "Test summary",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = [] // Required collection
            },
            new Analysis.Domain.Entities.Analysis
            {
                Id = 2,
                UserId = 1,
                DateAnalysis = endDate.AddDays(-1),
                ContentType = ContentType.html,
                ContentInput = "test content 2",
                SourceUrl = "https://test2.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.success,
                SummaryResult = "Test summary 2",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = [] // Required collection
            }
        };

        _context.Analyses.AddRange(analyses);
        _ = await _context.SaveChangesAsync();
    }

    private static AnalysisCreateDto CreateValidAnalysisCreateDto() => new(
        UserId: 1,
        DateAnalysis: DateTime.UtcNow,
        ContentType: "html",
        ContentInput: "test content",
        SourceUrl: "https://test.com",
        ToolUsed: "axecore",
        Status: "pending",
        SummaryResult: "Test summary",
        ResultJson: "{}",
        DurationMs: 1000,
        WcagVersion: "2.1",
        WcagLevel: "AA",
        AxeViolations: 0,
        AxeNeedsReview: 0,
        AxeRecommendations: 0,
        AxePasses: 10,
        AxeIncomplete: 0,
        AxeInapplicable: 0,
        EaViolations: 0,
        EaNeedsReview: 0,
        EaRecommendations: 0,
        EaPasses: 10,
        EaIncomplete: 0,
        EaInapplicable: 0
    );
}
