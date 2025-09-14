using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Analysis.Application.Services.Result;
using Analysis.Infrastructure.Data;
using Analysis.Domain.Entities;
using Analysis.Application.Dtos;

namespace Analysis.Tests.Application
{
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

        [Fact]
        public async Task GetByLevelAsync_ShouldReturnResultsWithSpecificLevel()
        {
            // Arrange
            var analysis = new Analysis.Domain.Entities.Analysis
            {
                Id = 1,
                UserId = 1,
                DateAnalysis = DateTime.UtcNow,
                ContentType = ContentType.html,
                SourceUrl = "https://example.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.success,
                SummaryResult = "Test",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = new List<Result>()
            };

            var result1 = new Result
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
                Analysis = analysis,
                Errors = new List<Analysis.Domain.Entities.Error>()
            };

            var result2 = new Result
            {
                Id = 2,
                AnalysisId = 1,
                WcagCriterionId = 2,
                WcagCriterion = "1.1.2",
                Level = ResultLevel.recommendation,
                Severity = Severity.medium,
                Description = "Test recommendation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Analysis = analysis,
                Errors = new List<Analysis.Domain.Entities.Error>()
            };

            _context.Analyses.Add(analysis);
            _context.Results.AddRange(result1, result2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByLevelAsync("violation");

            // Assert
            result.Should().HaveCount(1);
            result.First().Level.Should().Be("violation");
        }

        [Fact]
        public async Task GetBySeverityAsync_ShouldReturnResultsWithSpecificSeverity()
        {
            // Arrange
            var analysis = new Analysis.Domain.Entities.Analysis
            {
                Id = 1,
                UserId = 1,
                DateAnalysis = DateTime.UtcNow,
                ContentType = ContentType.html,
                SourceUrl = "https://example.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.success,
                SummaryResult = "Test",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = new List<Result>()
            };

            var result1 = new Result
            {
                Id = 1,
                AnalysisId = 1,
                WcagCriterionId = 1,
                WcagCriterion = "1.1.1",
                Level = ResultLevel.violation,
                Severity = Severity.high,
                Description = "High severity violation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Analysis = analysis,
                Errors = new List<Analysis.Domain.Entities.Error>()
            };

            var result2 = new Result
            {
                Id = 2,
                AnalysisId = 1,
                WcagCriterionId = 2,
                WcagCriterion = "1.1.2",
                Level = ResultLevel.recommendation,
                Severity = Severity.medium,
                Description = "Medium severity recommendation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Analysis = analysis,
                Errors = new List<Analysis.Domain.Entities.Error>()
            };

            _context.Analyses.Add(analysis);
            _context.Results.AddRange(result1, result2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBySeverityAsync("high");

            // Assert
            result.Should().HaveCount(1);
            result.First().Severity.Should().Be("high");
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateNewResult()
        {
            // Arrange
            var analysis = new Analysis.Domain.Entities.Analysis
            {
                Id = 1,
                UserId = 1,
                DateAnalysis = DateTime.UtcNow,
                ContentType = ContentType.html,
                SourceUrl = "https://example.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.success,
                SummaryResult = "Test",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = new List<Result>()
            };

            _context.Analyses.Add(analysis);
            await _context.SaveChangesAsync();

            var createDto = new ResultCreateDto(
                AnalysisId: 1,
                WcagCriterionId: 1,
                WcagCriterion: "1.1.1",
                Level: "violation",
                Severity: "high",
                Description: "Test violation"
            );

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be("violation");
            result.Severity.Should().Be("high");
        }

        [Fact]
        public async Task GetByAnalysisIdAsync_ShouldReturnResultsForAnalysis()
        {
            // Arrange
            var analysis = new Analysis.Domain.Entities.Analysis
            {
                Id = 1,
                UserId = 1,
                DateAnalysis = DateTime.UtcNow,
                ContentType = ContentType.html,
                SourceUrl = "https://example.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.success,
                SummaryResult = "Test",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = new List<Result>()
            };

            var result1 = new Result
            {
                AnalysisId = 1,
                WcagCriterionId = 1,
                WcagCriterion = "1.1.1",
                Level = ResultLevel.violation,
                Severity = Severity.high,
                Description = "Test violation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Analysis = analysis,
                Errors = new List<Analysis.Domain.Entities.Error>()
            };

            _context.Analyses.Add(analysis);
            _context.Results.Add(result1);
            await _context.SaveChangesAsync();

            // Act
            var results = await _service.GetByAnalysisIdAsync(1);

            // Assert
            results.Should().HaveCount(1);
            results.First().AnalysisId.Should().Be(1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}