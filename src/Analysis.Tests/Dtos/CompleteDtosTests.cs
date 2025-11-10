using Xunit;
using FluentAssertions;
using Analysis.Application.Dtos;

namespace Analysis.Tests.Dtos;

/// <summary>
/// Tests para los DTOs compuestos (CompleteAnalysisDto, CompleteResultDto, CompleteErrorDto).
/// </summary>
public class CompleteDtosTests
{
    #region CompleteErrorDto Tests

    [Fact]
    public void CompleteErrorDto_CanBeInstantiated_WithAllProperties()
    {
        // Arrange & Act
        var dto = new CompleteErrorDto(
            Id: 1,
            ResultId: 10,
            WcagCriterionId: 5,
            ErrorCode: "image-alt",
            Description: "Missing alt attribute",
            Location: "img.hero line 45",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.ResultId.Should().Be(10);
        dto.WcagCriterionId.Should().Be(5);
        dto.ErrorCode.Should().Be("image-alt");
        dto.Description.Should().Be("Missing alt attribute");
        dto.Location.Should().Be("img.hero line 45");
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        dto.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompleteErrorDto_IsImmutable()
    {
        // Arrange
        var dto1 = new CompleteErrorDto(1, 10, 5, "error-1", "Description", "Location", DateTime.UtcNow, DateTime.UtcNow);
        var dto2 = dto1 with { ErrorCode = "error-2" };

        // Assert
        dto1.ErrorCode.Should().Be("error-1");
        dto2.ErrorCode.Should().Be("error-2");
        dto1.Id.Should().Be(dto2.Id);
    }

    [Fact]
    public void CompleteErrorDto_EqualityWorks()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dto1 = new CompleteErrorDto(1, 10, 5, "error-1", "Description", "Location", now, now);
        var dto2 = new CompleteErrorDto(1, 10, 5, "error-1", "Description", "Location", now, now);
        var dto3 = new CompleteErrorDto(2, 10, 5, "error-1", "Description", "Location", now, now);

        // Assert
        dto1.Should().Be(dto2);
        dto1.Should().NotBe(dto3);
    }

    #endregion

    #region CompleteResultDto Tests

    [Fact]
    public void CompleteResultDto_CanBeInstantiated_WithAllProperties()
    {
        // Arrange
        var errors = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(1, 10, 5, "error-1", "Description 1", "Location 1", DateTime.UtcNow, DateTime.UtcNow),
            new CompleteErrorDto(2, 10, 5, "error-2", "Description 2", "Location 2", DateTime.UtcNow, DateTime.UtcNow)
        };

        // Act
        var dto = new CompleteResultDto(
            Id: 10,
            AnalysisId: 100,
            WcagCriterionId: 5,
            WcagCriterion: "1.1.1",
            Level: "A",
            Severity: "critical",
            Description: "Images must have alternate text",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Errors: errors
        );

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(10);
        dto.AnalysisId.Should().Be(100);
        dto.WcagCriterionId.Should().Be(5);
        dto.WcagCriterion.Should().Be("1.1.1");
        dto.Level.Should().Be("A");
        dto.Severity.Should().Be("critical");
        dto.Description.Should().Be("Images must have alternate text");
        dto.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void CompleteResultDto_WithEmptyErrors_IsValid()
    {
        // Act
        var dto = new CompleteResultDto(
            Id: 10,
            AnalysisId: 100,
            WcagCriterionId: 5,
            WcagCriterion: "1.1.1",
            Level: "A",
            Severity: "critical",
            Description: "Description",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Errors: new List<CompleteErrorDto>()
        );

        // Assert
        dto.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CompleteResultDto_IsImmutable()
    {
        // Arrange
        var errors1 = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(1, 10, 5, "error-1", "Description", "Location", DateTime.UtcNow, DateTime.UtcNow)
        };
        var errors2 = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(2, 10, 5, "error-2", "Description", "Location", DateTime.UtcNow, DateTime.UtcNow)
        };

        var dto1 = new CompleteResultDto(10, 100, 5, "1.1.1", "A", "critical", "Desc", DateTime.UtcNow, DateTime.UtcNow, errors1);
        var dto2 = dto1 with { Errors = errors2 };

        // Assert
        dto1.Errors.Should().HaveCount(1);
        dto2.Errors.Should().HaveCount(1);
        dto1.Errors.First().ErrorCode.Should().Be("error-1");
        dto2.Errors.First().ErrorCode.Should().Be("error-2");
    }

    [Fact]
    public void CompleteResultDto_CanAccessNestedErrors()
    {
        // Arrange
        var errors = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(1, 10, 5, "error-1", "Description 1", "Location 1", DateTime.UtcNow, DateTime.UtcNow),
            new CompleteErrorDto(2, 10, 5, "error-2", "Description 2", "Location 2", DateTime.UtcNow, DateTime.UtcNow)
        };

        var dto = new CompleteResultDto(10, 100, 5, "1.1.1", "A", "critical", "Desc", DateTime.UtcNow, DateTime.UtcNow, errors);

        // Act & Assert
        dto.Errors.First().ErrorCode.Should().Be("error-1");
        dto.Errors.Last().ErrorCode.Should().Be("error-2");
        dto.Errors.All(e => e.ResultId == 10).Should().BeTrue();
    }

    #endregion

    #region CompleteAnalysisDto Tests

    [Fact]
    public void CompleteAnalysisDto_CanBeInstantiated_WithAllProperties()
    {
        // Arrange
        var errors = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(1, 10, 5, "error-1", "Description", "Location", DateTime.UtcNow, DateTime.UtcNow)
        };

        var results = new List<CompleteResultDto>
        {
            new CompleteResultDto(10, 100, 5, "1.1.1", "A", "critical", "Desc", DateTime.UtcNow, DateTime.UtcNow, errors)
        };

        // Act
        var dto = new CompleteAnalysisDto(
            Id: 100,
            UserId: 123,
            DateAnalysis: DateTime.UtcNow,
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
            UpdatedAt: DateTime.UtcNow,
            Results: results
        );

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(100);
        dto.UserId.Should().Be(123);
        dto.ContentType.Should().Be("url");
        dto.SourceUrl.Should().Be("https://example.com");
        dto.ToolUsed.Should().Be("axe");
        dto.Status.Should().Be("completed");
        dto.WcagVersion.Should().Be("2.1");
        dto.WcagLevel.Should().Be("AA");
        dto.AxeViolations.Should().Be(10);
        dto.EaViolations.Should().Be(3);
        dto.Results.Should().HaveCount(1);
    }

    [Fact]
    public void CompleteAnalysisDto_WithEmptyResults_IsValid()
    {
        // Act
        var dto = new CompleteAnalysisDto(
            Id: 100,
            UserId: 123,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "url",
            ContentInput: null,
            SourceUrl: "https://example.com",
            ToolUsed: "axe",
            Status: "completed",
            SummaryResult: "No violations",
            ResultJson: "{}",
            DurationMs: 1000,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 0,
            AxeNeedsReview: 0,
            AxeRecommendations: 0,
            AxePasses: 100,
            AxeIncomplete: 0,
            AxeInapplicable: 0,
            EaViolations: null,
            EaNeedsReview: null,
            EaRecommendations: null,
            EaPasses: null,
            EaIncomplete: null,
            EaInapplicable: null,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Results: new List<CompleteResultDto>()
        );

        // Assert
        dto.Results.Should().BeEmpty();
        dto.AxeViolations.Should().Be(0);
    }

    [Fact]
    public void CompleteAnalysisDto_CanAccessNestedResultsAndErrors()
    {
        // Arrange
        var error1 = new CompleteErrorDto(1, 10, 5, "error-1", "Description 1", "Location 1", DateTime.UtcNow, DateTime.UtcNow);
        var error2 = new CompleteErrorDto(2, 10, 5, "error-2", "Description 2", "Location 2", DateTime.UtcNow, DateTime.UtcNow);

        var result1 = new CompleteResultDto(10, 100, 5, "1.1.1", "A", "critical", "Desc 1", DateTime.UtcNow, DateTime.UtcNow, new[] { error1 });
        var result2 = new CompleteResultDto(11, 100, 6, "1.2.1", "AA", "moderate", "Desc 2", DateTime.UtcNow, DateTime.UtcNow, new[] { error2 });

        var dto = new CompleteAnalysisDto(
            100, 123, DateTime.UtcNow, "url", null, "https://example.com", "axe", "completed",
            "Summary", "{}", 2500, "2.1", "AA", 10, 5, 2, 50, 1, 10, null, null, null, null, null, null,
            DateTime.UtcNow, DateTime.UtcNow, new[] { result1, result2 }
        );

        // Act & Assert
        dto.Results.Should().HaveCount(2);
        dto.Results.First().WcagCriterion.Should().Be("1.1.1");
        dto.Results.Last().WcagCriterion.Should().Be("1.2.1");
        dto.Results.First().Errors.First().ErrorCode.Should().Be("error-1");
        dto.Results.Last().Errors.First().ErrorCode.Should().Be("error-2");
    }

    [Fact]
    public void CompleteAnalysisDto_WithNullableFields_HandlesNullsCorrectly()
    {
        // Act
        var dto = new CompleteAnalysisDto(
            Id: 100,
            UserId: 123,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "html",
            ContentInput: "<html><body>Test</body></html>",
            SourceUrl: null, // Nullable
            ToolUsed: "axe",
            Status: "completed",
            SummaryResult: "Summary",
            ResultJson: "{}",
            DurationMs: null, // Nullable
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 10,
            AxeNeedsReview: null, // Nullable
            AxeRecommendations: null,
            AxePasses: null,
            AxeIncomplete: null,
            AxeInapplicable: null,
            EaViolations: null,
            EaNeedsReview: null,
            EaRecommendations: null,
            EaPasses: null,
            EaIncomplete: null,
            EaInapplicable: null,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Results: new List<CompleteResultDto>()
        );

        // Assert
        dto.SourceUrl.Should().BeNull();
        dto.DurationMs.Should().BeNull();
        dto.AxeNeedsReview.Should().BeNull();
        dto.EaViolations.Should().BeNull();
    }

    [Fact]
    public void CompleteAnalysisDto_IsImmutable()
    {
        // Arrange
        var results1 = new List<CompleteResultDto>
        {
            new CompleteResultDto(10, 100, 5, "1.1.1", "A", "critical", "Desc", DateTime.UtcNow, DateTime.UtcNow, new List<CompleteErrorDto>())
        };
        var results2 = new List<CompleteResultDto>
        {
            new CompleteResultDto(11, 100, 6, "1.2.1", "AA", "moderate", "Desc", DateTime.UtcNow, DateTime.UtcNow, new List<CompleteErrorDto>())
        };

        var dto1 = new CompleteAnalysisDto(
            100, 123, DateTime.UtcNow, "url", null, "https://example.com", "axe", "completed",
            "Summary", "{}", 2500, "2.1", "AA", 10, 5, 2, 50, 1, 10, null, null, null, null, null, null,
            DateTime.UtcNow, DateTime.UtcNow, results1
        );

        var dto2 = dto1 with { Results = results2 };

        // Assert
        dto1.Results.Should().HaveCount(1);
        dto2.Results.Should().HaveCount(1);
        dto1.Results.First().WcagCriterion.Should().Be("1.1.1");
        dto2.Results.First().WcagCriterion.Should().Be("1.2.1");
    }

    [Fact]
    public void CompleteAnalysisDto_CanCountTotalErrors()
    {
        // Arrange
        var errors1 = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(1, 10, 5, "error-1", "Desc", "Loc", DateTime.UtcNow, DateTime.UtcNow),
            new CompleteErrorDto(2, 10, 5, "error-2", "Desc", "Loc", DateTime.UtcNow, DateTime.UtcNow)
        };
        var errors2 = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(3, 11, 6, "error-3", "Desc", "Loc", DateTime.UtcNow, DateTime.UtcNow)
        };

        var results = new List<CompleteResultDto>
        {
            new CompleteResultDto(10, 100, 5, "1.1.1", "A", "critical", "Desc", DateTime.UtcNow, DateTime.UtcNow, errors1),
            new CompleteResultDto(11, 100, 6, "1.2.1", "AA", "moderate", "Desc", DateTime.UtcNow, DateTime.UtcNow, errors2)
        };

        var dto = new CompleteAnalysisDto(
            100, 123, DateTime.UtcNow, "url", null, "https://example.com", "axe", "completed",
            "Summary", "{}", 2500, "2.1", "AA", 10, 5, 2, 50, 1, 10, null, null, null, null, null, null,
            DateTime.UtcNow, DateTime.UtcNow, results
        );

        // Act
        var totalErrors = dto.Results.Sum(r => r.Errors.Count());

        // Assert
        totalErrors.Should().Be(3);
    }

    #endregion
}
