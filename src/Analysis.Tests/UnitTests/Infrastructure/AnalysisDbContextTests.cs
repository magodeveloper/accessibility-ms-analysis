using Microsoft.EntityFrameworkCore;
using Analysis.Domain.Entities;
using Analysis.Infrastructure.Data;
using Xunit;
using FluentAssertions;

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
        _context.Analyses.Should().NotBeNull();
        _context.Results.Should().NotBeNull();
        _context.Errors.Should().NotBeNull();
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
            Results = new List<Result>()
        };

        // Act
        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        // Assert
        var savedAnalysis = await _context.Analyses.FirstOrDefaultAsync(a => a.SourceUrl == "https://test.com");
        savedAnalysis.Should().NotBeNull();
        savedAnalysis!.UserId.Should().Be(100);
        savedAnalysis.Status.Should().Be(AnalysisStatus.success);
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
            Results = new List<Result>()
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
            Errors = new List<Error>()
        };

        // Act
        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        result.AnalysisId = analysis.Id;
        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        // Assert
        var savedResult = await _context.Results
            .Include(r => r.Analysis)
            .FirstOrDefaultAsync(r => r.WcagCriterion == "1.1.1 Non-text Content");

        savedResult.Should().NotBeNull();
        savedResult!.Description.Should().Be("Missing alt text on image");
        savedResult.Severity.Should().Be(Severity.high);
        savedResult.Analysis.Should().NotBeNull();
        savedResult.Analysis.SourceUrl.Should().Be("https://result-test.com");
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
            Results = new List<Result>()
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
            Errors = new List<Error>()
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
            Errors = new List<Error>()
        };

        analysis.Results.Add(result1);
        analysis.Results.Add(result2);

        // Act
        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        // Assert
        var savedAnalysis = await _context.Analyses
            .Include(a => a.Results)
            .FirstOrDefaultAsync(a => a.SourceUrl == "https://relationship-test.com");

        savedAnalysis.Should().NotBeNull();
        savedAnalysis!.Results.Should().HaveCount(2);
        savedAnalysis.Results.Should().Contain(r => r.WcagCriterion == "1.1.1 Non-text Content");
        savedAnalysis.Results.Should().Contain(r => r.WcagCriterion == "1.3.1 Info and Relationships");
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
            Results = new List<Result>()
        };

        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = analysis.UpdatedAt;

        // Wait a moment to ensure timestamp difference
        await Task.Delay(1);

        // Act
        analysis.Status = AnalysisStatus.success;
        analysis.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert
        var updatedAnalysis = await _context.Analyses.FindAsync(analysis.Id);
        updatedAnalysis.Should().NotBeNull();
        updatedAnalysis!.Status.Should().Be(AnalysisStatus.success);
        updatedAnalysis.UpdatedAt.Should().BeAfter(originalUpdatedAt);
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