using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Analysis.Infrastructure.Data;
using Analysis.Domain.Entities;
using Analysis.Application.Services.Analysis;
using Analysis.Application.Dtos;
using Analysis.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using AnalysisEntity = Analysis.Domain.Entities.Analysis;

namespace Analysis.Tests.Application;

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
        _context.Database.EnsureCreated();
        _mockUserValidationService = new Mock<IUserValidationService>();
        _mockLogger = new Mock<ILogger<AnalysisService>>();
        _service = new AnalysisService(_context, _mockUserValidationService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnAnalyses()
    {
        // Arrange
        var analysis1 = new AnalysisEntity
        {
            UserId = 1,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.html,
            ContentInput = "<html></html>",
            SourceUrl = "https://example.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.success,
            SummaryResult = "Analysis completed",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = new List<Analysis.Domain.Entities.Result>()
        };

        var analysis2 = new AnalysisEntity
        {
            UserId = 2,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.url,
            ContentInput = "https://test.com",
            SourceUrl = "https://test.com",
            ToolUsed = ToolUsed.equalaccess,
            Status = AnalysisStatus.pending,
            SummaryResult = "Pending analysis",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = new List<Analysis.Domain.Entities.Result>()
        };

        _context.Analyses.AddRange(analysis1, analysis2);
        await _context.SaveChangesAsync();

        _mockUserValidationService.Setup(x => x.ValidateUserExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GetByUserIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAnalysis()
    {
        // Arrange
        var createDto = new AnalysisCreateDto(
            UserId: 1,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "html",
            ContentInput: "<html></html>",
            SourceUrl: "https://example.com",
            ToolUsed: "axe-core",
            Status: "success",
            SummaryResult: "Pending analysis",
            ResultJson: "{}",
            DurationMs: 100,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 0,
            AxeNeedsReview: 0,
            AxeRecommendations: 0,
            AxePasses: 5,
            AxeIncomplete: 0,
            AxeInapplicable: 0,
            EaViolations: 0,
            EaNeedsReview: 0,
            EaRecommendations: 0,
            EaPasses: 5,
            EaIncomplete: 0,
            EaInapplicable: 0
        );

        _mockUserValidationService
            .Setup(x => x.ValidateUserExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(createDto.UserId);
        result.ContentType.Should().Be(createDto.ContentType);

        var savedAnalysis = await _context.Analyses.FirstOrDefaultAsync();
        savedAnalysis.Should().NotBeNull();
        savedAnalysis!.UserId.Should().Be(createDto.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnAnalysis()
    {
        // Arrange
        var analysis = new AnalysisEntity
        {
            UserId = 1,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.html,
            ContentInput = "<html></html>",
            SourceUrl = "https://example.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.success,
            SummaryResult = "Analysis completed",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = new List<Analysis.Domain.Entities.Result>()
        };

        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(analysis.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(analysis.Id);
        result.UserId.Should().Be(analysis.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAnalyses()
    {
        // Arrange
        var analyses = new[]
        {
            CreateSampleAnalysis(1, 1),
            CreateSampleAnalysis(2, 2),
            CreateSampleAnalysis(3, 1)
        };

        _context.Analyses.AddRange(analyses);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteAnalysis()
    {
        // Arrange
        var analysis = CreateSampleAnalysis(1, 1);
        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(analysis.Id);

        // Assert
        var deletedAnalysis = await _context.Analyses.FindAsync(analysis.Id);
        deletedAnalysis.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldDeleteAllAnalyses()
    {
        // Arrange
        var analyses = new[]
        {
            CreateSampleAnalysis(1, 1),
            CreateSampleAnalysis(2, 2)
        };

        _context.Analyses.AddRange(analyses);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAllAsync();

        // Assert
        var remainingAnalyses = await _context.Analyses.ToListAsync();
        remainingAnalyses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDateAsync_ShouldReturnAnalysesForSpecificDate()
    {
        // Arrange
        var targetDate = DateTime.Today;
        var analyses = new[]
        {
            CreateSampleAnalysis(1, 1, targetDate),
            CreateSampleAnalysis(2, 1, targetDate.AddDays(1)),
            CreateSampleAnalysis(3, 1, targetDate)
        };

        _context.Analyses.AddRange(analyses);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByDateAsync(1, targetDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.DateAnalysis.Date == targetDate.Date);
    }

    private static AnalysisEntity CreateSampleAnalysis(int id, int userId, DateTime? dateAnalysis = null)
    {
        return new AnalysisEntity
        {
            Id = id,
            UserId = userId,
            DateAnalysis = dateAnalysis ?? DateTime.UtcNow,
            ContentType = ContentType.html,
            ContentInput = "<html></html>",
            SourceUrl = "https://example.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.success,
            SummaryResult = "Analysis completed",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = new List<Analysis.Domain.Entities.Result>()
        };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}