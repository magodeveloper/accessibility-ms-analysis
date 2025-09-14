using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Analysis.Application.Services.Error;
using Analysis.Infrastructure.Data;
using Analysis.Domain.Entities;
using Analysis.Application.Dtos;

namespace Analysis.Tests.Application
{
    public class ErrorServiceTests : IDisposable
    {
        private readonly AnalysisDbContext _context;
        private readonly ErrorService _service;

        public ErrorServiceTests()
        {
            var options = new DbContextOptionsBuilder<AnalysisDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AnalysisDbContext(options);
            _service = new ErrorService(_context);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnError()
        {
            // Arrange
            var analysis = CreateSampleAnalysis();
            var result = CreateSampleResult(analysis);
            var error = new Analysis.Domain.Entities.Error
            {
                Id = 1,
                ResultId = result.Id,
                WcagCriterionId = 1,
                ErrorCode = "E001",
                Description = "Test error",
                Location = "line 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = result
            };

            _context.Analyses.Add(analysis);
            _context.Results.Add(result);
            _context.Errors.Add(error);
            await _context.SaveChangesAsync();

            // Act
            var resultError = await _service.GetByIdAsync(1);

            // Assert
            resultError.Should().NotBeNull();
            resultError!.Id.Should().Be(1);
            resultError.ErrorCode.Should().Be("E001");
        }

        [Fact]
        public async Task GetByResultIdAsync_ShouldReturnErrorsForResult()
        {
            // Arrange
            var analysis = CreateSampleAnalysis();
            var result = CreateSampleResult(analysis);
            var error1 = new Analysis.Domain.Entities.Error
            {
                ResultId = result.Id,
                WcagCriterionId = 1,
                ErrorCode = "E001",
                Description = "Test error 1",
                Location = "line 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = result
            };

            var error2 = new Analysis.Domain.Entities.Error
            {
                ResultId = result.Id,
                WcagCriterionId = 2,
                ErrorCode = "E002",
                Description = "Test error 2",
                Location = "line 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = result
            };

            _context.Analyses.Add(analysis);
            _context.Results.Add(result);
            _context.Errors.AddRange(error1, error2);
            await _context.SaveChangesAsync();

            // Act
            var errors = await _service.GetByResultIdAsync(result.Id);

            // Assert
            errors.Should().HaveCount(2);
            errors.Should().OnlyContain(e => e.ResultId == result.Id);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateNewError()
        {
            // Arrange
            var analysis = CreateSampleAnalysis();
            var result = CreateSampleResult(analysis);

            _context.Analyses.Add(analysis);
            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            var createDto = new ErrorCreateDto(
                ResultId: result.Id,
                WcagCriterionId: 1,
                ErrorCode: "E001",
                Description: "Test error",
                Location: "line 1"
            );

            // Act
            var error = await _service.CreateAsync(createDto);

            // Assert
            error.Should().NotBeNull();
            error.ErrorCode.Should().Be("E001");
            error.ResultId.Should().Be(result.Id);

            var savedError = await _context.Errors.FirstOrDefaultAsync();
            savedError.Should().NotBeNull();
            savedError!.ErrorCode.Should().Be("E001");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllErrors()
        {
            // Arrange
            var analysis = CreateSampleAnalysis();
            var result = CreateSampleResult(analysis);

            var error1 = new Analysis.Domain.Entities.Error
            {
                ResultId = result.Id,
                WcagCriterionId = 1,
                ErrorCode = "E001",
                Description = "Test error 1",
                Location = "line 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = result
            };

            var error2 = new Analysis.Domain.Entities.Error
            {
                ResultId = result.Id,
                WcagCriterionId = 2,
                ErrorCode = "E002",
                Description = "Test error 2",
                Location = "line 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = result
            };

            _context.Analyses.Add(analysis);
            _context.Results.Add(result);
            _context.Errors.AddRange(error1, error2);
            await _context.SaveChangesAsync();

            // Act
            var errors = await _service.GetAllAsync();

            // Assert
            errors.Should().HaveCount(2);
        }

        private Analysis.Domain.Entities.Analysis CreateSampleAnalysis()
        {
            return new Analysis.Domain.Entities.Analysis
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
        }

        private Result CreateSampleResult(Analysis.Domain.Entities.Analysis analysis)
        {
            return new Result
            {
                Id = 1,
                AnalysisId = analysis.Id,
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
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}