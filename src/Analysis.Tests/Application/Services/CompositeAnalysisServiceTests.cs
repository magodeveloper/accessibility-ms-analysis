using Moq;
using Xunit;
using FluentAssertions;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.Analysis;
using Analysis.Application.Services.Composite;

namespace Analysis.Tests.Application.Services;

/// <summary>
/// Tests unitarios para CompositeAnalysisService.
/// </summary>
public class CompositeAnalysisServiceTests
{
    private readonly Mock<IAnalysisService> _mockAnalysisService;
    private readonly Mock<IResultService> _mockResultService;
    private readonly Mock<IErrorService> _mockErrorService;
    private readonly CompositeAnalysisService _service;

    public CompositeAnalysisServiceTests()
    {
        _mockAnalysisService = new Mock<IAnalysisService>();
        _mockResultService = new Mock<IResultService>();
        _mockErrorService = new Mock<IErrorService>();

        _service = new CompositeAnalysisService(
            _mockAnalysisService.Object,
            _mockResultService.Object,
            _mockErrorService.Object
        );
    }

    #region GetCompleteAnalysisByIdAsync Tests

    [Fact]
    public async Task GetCompleteAnalysisByIdAsync_WithValidId_ReturnsCompleteAnalysis()
    {
        // Arrange
        var analysisId = 1;
        var analysis = CreateSampleAnalysis(analysisId, userId: 123);
        var results = CreateSampleResults(analysisId, count: 2);
        var errors1 = CreateSampleErrors(resultId: results[0].Id, count: 2);
        var errors2 = CreateSampleErrors(resultId: results[results.Count - 1].Id, count: 1);

        _mockAnalysisService
            .Setup(x => x.GetByIdAsync(analysisId))
            .ReturnsAsync(analysis);

        _mockResultService
            .Setup(x => x.GetByAnalysisIdAsync(analysisId))
            .ReturnsAsync(results);

        _mockErrorService
            .Setup(x => x.GetByResultIdAsync(results[0].Id))
            .ReturnsAsync(errors1);

        _mockErrorService
            .Setup(x => x.GetByResultIdAsync(results[results.Count - 1].Id))
            .ReturnsAsync(errors2);

        // Act
        var result = await _service.GetCompleteAnalysisByIdAsync(analysisId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(analysisId);
        result.UserId.Should().Be(123);
        result.Results.Should().HaveCount(2);
        result.Results.First().Errors.Should().HaveCount(2);
        result.Results.Last().Errors.Should().HaveCount(1);

        // Verify all services were called
        _mockAnalysisService.Verify(x => x.GetByIdAsync(analysisId), Times.Once);
        _mockResultService.Verify(x => x.GetByAnalysisIdAsync(analysisId), Times.Once);
        _mockErrorService.Verify(x => x.GetByResultIdAsync(It.IsAny<int>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetCompleteAnalysisByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var analysisId = 999;
        _mockAnalysisService
            .Setup(x => x.GetByIdAsync(analysisId))
            .ReturnsAsync((AnalysisDto?)null);

        // Act
        var result = await _service.GetCompleteAnalysisByIdAsync(analysisId);

        // Assert
        result.Should().BeNull();

        // Verify only analysis service was called
        _mockAnalysisService.Verify(x => x.GetByIdAsync(analysisId), Times.Once);
        _mockResultService.Verify(x => x.GetByAnalysisIdAsync(It.IsAny<int>()), Times.Never);
        _mockErrorService.Verify(x => x.GetByResultIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetCompleteAnalysisByIdAsync_WithNoResults_ReturnsAnalysisWithEmptyResults()
    {
        // Arrange
        var analysisId = 1;
        var analysis = CreateSampleAnalysis(analysisId, userId: 123);
        var emptyResults = new List<ResultDto>();

        _mockAnalysisService
            .Setup(x => x.GetByIdAsync(analysisId))
            .ReturnsAsync(analysis);

        _mockResultService
            .Setup(x => x.GetByAnalysisIdAsync(analysisId))
            .ReturnsAsync(emptyResults);

        // Act
        var result = await _service.GetCompleteAnalysisByIdAsync(analysisId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(analysisId);
        result.Results.Should().BeEmpty();

        // Verify error service was never called
        _mockErrorService.Verify(x => x.GetByResultIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetCompleteAnalysisByIdAsync_WithResultsButNoErrors_ReturnsCompleteAnalysisWithEmptyErrors()
    {
        // Arrange
        var analysisId = 1;
        var analysis = CreateSampleAnalysis(analysisId, userId: 123);
        var results = CreateSampleResults(analysisId, count: 2);
        var emptyErrors = new List<ErrorDto>();

        _mockAnalysisService
            .Setup(x => x.GetByIdAsync(analysisId))
            .ReturnsAsync(analysis);

        _mockResultService
            .Setup(x => x.GetByAnalysisIdAsync(analysisId))
            .ReturnsAsync(results);

        _mockErrorService
            .Setup(x => x.GetByResultIdAsync(It.IsAny<int>()))
            .ReturnsAsync(emptyErrors);

        // Act
        var result = await _service.GetCompleteAnalysisByIdAsync(analysisId);

        // Assert
        result.Should().NotBeNull();
        result!.Results.Should().HaveCount(2);
        result.Results.First().Errors.Should().BeEmpty();
        result.Results.Last().Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompleteAnalysisByIdAsync_MapsAllAnalysisPropertiesCorrectly()
    {
        // Arrange
        var analysisId = 1;
        var analysis = new AnalysisDto(
            Id: analysisId,
            UserId: 123,
            DateAnalysis: new DateTime(2025, 11, 10, 10, 30, 0),
            ContentType: "url",
            ContentInput: null,
            SourceUrl: "https://example.com",
            ToolUsed: "axe",
            Status: "completed",
            SummaryResult: "10 violations found",
            ResultJson: "{}",
            DurationMs: 2500,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 10,
            AxeNeedsReview: 5,
            AxeRecommendations: 2,
            AxePasses: 50,
            AxeIncomplete: 1,
            AxeInapplicable: 10,
            EaViolations: 3,
            EaNeedsReview: 2,
            EaRecommendations: 1,
            EaPasses: 40,
            EaIncomplete: 0,
            EaInapplicable: 5,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );

        _mockAnalysisService.Setup(x => x.GetByIdAsync(analysisId)).ReturnsAsync(analysis);
        _mockResultService.Setup(x => x.GetByAnalysisIdAsync(analysisId)).ReturnsAsync(new List<ResultDto>());

        // Act
        var result = await _service.GetCompleteAnalysisByIdAsync(analysisId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(analysisId);
        result.UserId.Should().Be(123);
        result.ContentType.Should().Be("url");
        result.SourceUrl.Should().Be("https://example.com");
        result.ToolUsed.Should().Be("axe");
        result.Status.Should().Be("completed");
        result.WcagVersion.Should().Be("2.1");
        result.WcagLevel.Should().Be("AA");
        result.AxeViolations.Should().Be(10);
        result.EaViolations.Should().Be(3);
    }

    #endregion

    #region GetCompleteAnalysesByUserIdAsync Tests

    [Fact]
    public async Task GetCompleteAnalysesByUserIdAsync_WithValidUserId_ReturnsAllUserAnalyses()
    {
        // Arrange
        var userId = 123;
        var analyses = new List<AnalysisDto>
        {
            CreateSampleAnalysis(1, userId),
            CreateSampleAnalysis(2, userId),
            CreateSampleAnalysis(3, userId)
        };

        _mockAnalysisService
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(analyses);

        // Setup para cada análisis individual
        foreach (var analysis in analyses)
        {
            _mockAnalysisService
                .Setup(x => x.GetByIdAsync(analysis.Id))
                .ReturnsAsync(analysis);

            _mockResultService
                .Setup(x => x.GetByAnalysisIdAsync(analysis.Id))
                .ReturnsAsync(new List<ResultDto>());
        }

        // Act
        var result = await _service.GetCompleteAnalysesByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.All(a => a.UserId == userId).Should().BeTrue();

        // Verify services were called correctly
        _mockAnalysisService.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        _mockAnalysisService.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserIdAsync_WithUserWithoutAnalyses_ReturnsEmptyList()
    {
        // Arrange
        var userId = 999;
        _mockAnalysisService
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<AnalysisDto>());

        // Act
        var result = await _service.GetCompleteAnalysesByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        // Verify only user query was made
        _mockAnalysisService.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        _mockAnalysisService.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserIdAsync_WithMultipleAnalysesAndResults_ReturnsCompleteData()
    {
        // Arrange
        var userId = 123;
        var analyses = new List<AnalysisDto>
        {
            CreateSampleAnalysis(1, userId),
            CreateSampleAnalysis(2, userId)
        };

        var results1 = CreateSampleResults(1, count: 2);
        var results2 = CreateSampleResults(2, count: 3);
        var errors = CreateSampleErrors(resultId: 1, count: 1);

        _mockAnalysisService.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(analyses);
        _mockAnalysisService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(analyses[0]);
        _mockAnalysisService.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(analyses[1]);
        _mockResultService.Setup(x => x.GetByAnalysisIdAsync(1)).ReturnsAsync(results1);
        _mockResultService.Setup(x => x.GetByAnalysisIdAsync(2)).ReturnsAsync(results2);
        _mockErrorService.Setup(x => x.GetByResultIdAsync(It.IsAny<int>())).ReturnsAsync(errors);

        // Act
        var result = await _service.GetCompleteAnalysesByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Results.Should().HaveCount(2);
        result.Last().Results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserIdAsync_CallsGetCompleteAnalysisByIdForEachAnalysis()
    {
        // Arrange
        var userId = 123;
        var analyses = new List<AnalysisDto>
        {
            CreateSampleAnalysis(1, userId),
            CreateSampleAnalysis(2, userId)
        };

        _mockAnalysisService.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(analyses);

        foreach (var analysis in analyses)
        {
            _mockAnalysisService.Setup(x => x.GetByIdAsync(analysis.Id)).ReturnsAsync(analysis);
            _mockResultService.Setup(x => x.GetByAnalysisIdAsync(analysis.Id)).ReturnsAsync(new List<ResultDto>());
        }

        // Act
        await _service.GetCompleteAnalysesByUserIdAsync(userId);

        // Assert
        _mockAnalysisService.Verify(x => x.GetByIdAsync(1), Times.Once);
        _mockAnalysisService.Verify(x => x.GetByIdAsync(2), Times.Once);
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserIdAsync_SkipsNullAnalyses()
    {
        // Arrange
        var userId = 123;
        var analyses = new List<AnalysisDto>
        {
            CreateSampleAnalysis(1, userId),
            CreateSampleAnalysis(2, userId),
            CreateSampleAnalysis(3, userId)
        };

        _mockAnalysisService.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(analyses);

        // El segundo análisis retorna null (fue eliminado entre consultas)
        _mockAnalysisService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(analyses[0]);
        _mockAnalysisService.Setup(x => x.GetByIdAsync(2)).ReturnsAsync((AnalysisDto?)null);
        _mockAnalysisService.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(analyses[2]);

        _mockResultService.Setup(x => x.GetByAnalysisIdAsync(It.IsAny<int>())).ReturnsAsync(new List<ResultDto>());

        // Act
        var result = await _service.GetCompleteAnalysesByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2); // Solo 2 porque el análisis 2 retornó null
        result.Should().OnlyContain(a => a.Id == 1 || a.Id == 3);
    }

    #endregion

    #region Helper Methods

    private static AnalysisDto CreateSampleAnalysis(int id, int userId)
    {
        return new AnalysisDto(
            Id: id,
            UserId: userId,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "url",
            ContentInput: null,
            SourceUrl: $"https://example{id}.com",
            ToolUsed: "axe",
            Status: "completed",
            SummaryResult: $"{id * 10} violations found",
            ResultJson: "{}",
            DurationMs: 2500,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: id * 10,
            AxeNeedsReview: id * 5,
            AxeRecommendations: id * 2,
            AxePasses: id * 50,
            AxeIncomplete: id,
            AxeInapplicable: id * 10,
            EaViolations: null,
            EaNeedsReview: null,
            EaRecommendations: null,
            EaPasses: null,
            EaIncomplete: null,
            EaInapplicable: null,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );
    }

    private static List<ResultDto> CreateSampleResults(int analysisId, int count)
    {
        var results = new List<ResultDto>();
        for (int i = 1; i <= count; i++)
        {
            results.Add(new ResultDto(
                Id: i,
                AnalysisId: analysisId,
                WcagCriterionId: i,
                WcagCriterion: $"1.{i}.1",
                Level: "A",
                Severity: "critical",
                Description: $"Result {i} description",
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow
            ));
        }
        return results;
    }

    private static List<ErrorDto> CreateSampleErrors(int resultId, int count)
    {
        var errors = new List<ErrorDto>();
        for (int i = 1; i <= count; i++)
        {
            errors.Add(new ErrorDto(
                Id: i,
                ResultId: resultId,
                WcagCriterionId: i,
                ErrorCode: $"error-{i}",
                Description: $"Error {i} description",
                Location: $"element#{i} line {i * 10}",
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow
            ));
        }
        return errors;
    }

    #endregion
}
