using FluentAssertions;
using Analysis.Domain.Entities;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analysis.Tests.UnitTests.Infrastructure;

public class AnalysisDbContextTests : IDisposable
{
    private readonly AnalysisDbContext _context;
    private bool _disposed;

    public AnalysisDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        _context = new AnalysisDbContext(options);
    }

    [Fact]
    public void DbContext_ShouldCreateAnalysisDbSets()
    {
        // Assert
        _ = _context.Analyses.Should().NotBeNull();
        _ = _context.Results.Should().NotBeNull();
        _ = _context.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_ShouldSaveAndRetrieveAnalysis()
    {
        // Arrange
        var analysis = new Analysis.Domain.Entities.Analysis
        {
            UserId = 100,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.url,
            SourceUrl = "https://test.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.success,
            SummaryResult = "Test summary",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = []
        };

        // Act
        _ = _context.Analyses.Add(analysis);
        _ = await _context.SaveChangesAsync();

        // Assert
        var savedAnalysis = await _context.Analyses.FirstOrDefaultAsync(a => a.SourceUrl == "https://test.com");
        _ = savedAnalysis.Should().NotBeNull();
        _ = savedAnalysis!.UserId.Should().Be(100);
        _ = savedAnalysis.Status.Should().Be(AnalysisStatus.success);
    }

    [Fact]
    public async Task DbContext_ShouldSaveAndRetrieveResult()
    {
        // Arrange
        var analysis = new Analysis.Domain.Entities.Analysis
        {
            UserId = 200,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.url,
            SourceUrl = "https://result-test.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.success,
            SummaryResult = "Test summary",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = []
        };

        var result = new Result
        {
            Analysis = analysis,
            WcagCriterionId = 1,
            WcagCriterion = "1.1.1 Non-text Content",
            Level = ResultLevel.violation,
            Severity = Severity.high,
            Description = "Missing alt text on image",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Errors = []
        };

        // Act
        _ = _context.Analyses.Add(analysis);
        _ = await _context.SaveChangesAsync();

        result.AnalysisId = analysis.Id;
        _ = _context.Results.Add(result);
        _ = await _context.SaveChangesAsync();

        // Assert
        var savedResult = await _context.Results
            .Include(r => r.Analysis)
            .FirstOrDefaultAsync(r => r.WcagCriterion == "1.1.1 Non-text Content");

        _ = savedResult.Should().NotBeNull();
        _ = savedResult!.Description.Should().Be("Missing alt text on image");
        _ = savedResult.Severity.Should().Be(Severity.high);
        _ = savedResult.Analysis.Should().NotBeNull();
        _ = savedResult.Analysis.SourceUrl.Should().Be("https://result-test.com");
    }

    [Fact]
    public async Task DbContext_ShouldHandleRelationships()
    {
        // Arrange
        var analysis = new Analysis.Domain.Entities.Analysis
        {
            UserId = 400,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.url,
            SourceUrl = "https://relationship-test.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.success,
            SummaryResult = "Test summary",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = []
        };

        var result1 = new Result
        {
            Analysis = analysis,
            WcagCriterionId = 1,
            WcagCriterion = "1.1.1 Non-text Content",
            Level = ResultLevel.violation,
            Severity = Severity.high,
            Description = "Missing alt text on image",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Errors = []
        };

        var result2 = new Result
        {
            Analysis = analysis,
            WcagCriterionId = 2,
            WcagCriterion = "1.3.1 Info and Relationships",
            Level = ResultLevel.violation,
            Severity = Severity.medium,
            Description = "Missing label for input",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Errors = []
        };

        analysis.Results.Add(result1);
        analysis.Results.Add(result2);

        // Act
        _ = _context.Analyses.Add(analysis);
        _ = await _context.SaveChangesAsync();

        // Assert
        var savedAnalysis = await _context.Analyses
            .Include(a => a.Results)
            .FirstOrDefaultAsync(a => a.SourceUrl == "https://relationship-test.com");

        _ = savedAnalysis.Should().NotBeNull();
        _ = savedAnalysis!.Results.Should().HaveCount(2);
        _ = savedAnalysis.Results.Should().Contain(r => r.WcagCriterion == "1.1.1 Non-text Content");
        _ = savedAnalysis.Results.Should().Contain(r => r.WcagCriterion == "1.3.1 Info and Relationships");
    }

    [Fact]
    public async Task DbContext_ShouldUpdateTimestamps()
    {
        // Arrange
        var analysis = new Analysis.Domain.Entities.Analysis
        {
            UserId = 500,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.url,
            SourceUrl = "https://timestamp-test.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.pending,
            SummaryResult = "Test summary",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Results = []
        };

        _ = _context.Analyses.Add(analysis);
        _ = await _context.SaveChangesAsync();

        var originalUpdatedAt = analysis.UpdatedAt;

        // Wait a moment to ensure timestamp difference
        await Task.Delay(1);

        // Act
        analysis.Status = AnalysisStatus.success;
        analysis.UpdatedAt = DateTime.UtcNow;
        _ = await _context.SaveChangesAsync();

        // Assert
        var updatedAnalysis = await _context.Analyses.FindAsync(analysis.Id);
        _ = updatedAnalysis.Should().NotBeNull();
        _ = updatedAnalysis!.Status.Should().Be(AnalysisStatus.success);
        _ = updatedAnalysis.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }
}
