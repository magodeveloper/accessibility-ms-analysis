using FluentAssertions;
using Analysis.Application.Dtos;

namespace Analysis.Tests.Dtos;

public class DtoInstantiationTests
{
    [Fact]
    public void AnalysisPatchDto_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new AnalysisPatchDto(
            ContentType: "text/html",
            ContentInput: "<html><body>Test content</body></html>",
            SourceUrl: "https://example.com",
            ToolUsed: "axe-core",
            Status: "completed",
            SummaryResult: "Analysis completed successfully",
            ResultJson: "{}",
            DurationMs: 1500,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 5,
            AxeNeedsReview: 2,
            AxeRecommendations: 8,
            AxePasses: 15,
            AxeIncomplete: 1,
            AxeInapplicable: 3,
            EaViolations: 4,
            EaNeedsReview: 1,
            EaRecommendations: 6,
            EaPasses: 12,
            EaIncomplete: 0,
            EaInapplicable: 2
        );

        // Assert
        _ = dto.Should().NotBeNull();
        _ = dto.ContentType.Should().Be("text/html");
        _ = dto.ContentInput.Should().Be("<html><body>Test content</body></html>");
        _ = dto.SourceUrl.Should().Be("https://example.com");
        _ = dto.ToolUsed.Should().Be("axe-core");
        _ = dto.Status.Should().Be("completed");
        _ = dto.SummaryResult.Should().Be("Analysis completed successfully");
        _ = dto.ResultJson.Should().Be("{}");
        _ = dto.DurationMs.Should().Be(1500);
        _ = dto.WcagVersion.Should().Be("2.1");
        _ = dto.WcagLevel.Should().Be("AA");
        _ = dto.AxeViolations.Should().Be(5);
        _ = dto.AxeNeedsReview.Should().Be(2);
        _ = dto.AxeRecommendations.Should().Be(8);
        _ = dto.AxePasses.Should().Be(15);
        _ = dto.AxeIncomplete.Should().Be(1);
        _ = dto.AxeInapplicable.Should().Be(3);
        _ = dto.EaViolations.Should().Be(4);
        _ = dto.EaNeedsReview.Should().Be(1);
        _ = dto.EaRecommendations.Should().Be(6);
        _ = dto.EaPasses.Should().Be(12);
        _ = dto.EaIncomplete.Should().Be(0);
        _ = dto.EaInapplicable.Should().Be(2);
    }

    [Fact]
    public void AnalysisPatchDto_WithNullValues_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new AnalysisPatchDto(
            ContentType: null,
            ContentInput: null,
            SourceUrl: null,
            ToolUsed: null,
            Status: null,
            SummaryResult: null,
            ResultJson: null,
            DurationMs: null,
            WcagVersion: null,
            WcagLevel: null,
            AxeViolations: null,
            AxeNeedsReview: null,
            AxeRecommendations: null,
            AxePasses: null,
            AxeIncomplete: null,
            AxeInapplicable: null,
            EaViolations: null,
            EaNeedsReview: null,
            EaRecommendations: null,
            EaPasses: null,
            EaIncomplete: null,
            EaInapplicable: null
        );

        // Assert
        _ = dto.Should().NotBeNull();
        _ = dto.ContentType.Should().BeNull();
        _ = dto.ContentInput.Should().BeNull();
        _ = dto.SourceUrl.Should().BeNull();
        _ = dto.ToolUsed.Should().BeNull();
        _ = dto.Status.Should().BeNull();
        _ = dto.SummaryResult.Should().BeNull();
        _ = dto.ResultJson.Should().BeNull();
        _ = dto.DurationMs.Should().BeNull();
        _ = dto.WcagVersion.Should().BeNull();
        _ = dto.WcagLevel.Should().BeNull();
        _ = dto.AxeViolations.Should().BeNull();
        _ = dto.AxeNeedsReview.Should().BeNull();
        _ = dto.AxeRecommendations.Should().BeNull();
        _ = dto.AxePasses.Should().BeNull();
        _ = dto.AxeIncomplete.Should().BeNull();
        _ = dto.AxeInapplicable.Should().BeNull();
        _ = dto.EaViolations.Should().BeNull();
        _ = dto.EaNeedsReview.Should().BeNull();
        _ = dto.EaRecommendations.Should().BeNull();
        _ = dto.EaPasses.Should().BeNull();
        _ = dto.EaIncomplete.Should().BeNull();
        _ = dto.EaInapplicable.Should().BeNull();
    }

    [Fact]
    public void ErrorPatchDto_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new ErrorPatchDto(
            ErrorCode: "WCAG21_AA_001",
            Description: "Missing alt text for image",
            Location: "element[img]:nth-child(1)"
        );

        // Assert
        _ = dto.Should().NotBeNull();
        _ = dto.ErrorCode.Should().Be("WCAG21_AA_001");
        _ = dto.Description.Should().Be("Missing alt text for image");
        _ = dto.Location.Should().Be("element[img]:nth-child(1)");
    }

    [Fact]
    public void ErrorPatchDto_WithNullValues_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new ErrorPatchDto(
            ErrorCode: null,
            Description: null,
            Location: null
        );

        // Assert
        _ = dto.Should().NotBeNull();
        _ = dto.ErrorCode.Should().BeNull();
        _ = dto.Description.Should().BeNull();
        _ = dto.Location.Should().BeNull();
    }

    [Fact]
    public void ResultPatchDto_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new ResultPatchDto(
            WcagCriterion: "1.1.1 Non-text Content",
            Level: "A",
            Severity: "critical",
            Description: "Images must have alternative text"
        );

        // Assert
        _ = dto.Should().NotBeNull();
        _ = dto.WcagCriterion.Should().Be("1.1.1 Non-text Content");
        _ = dto.Level.Should().Be("A");
        _ = dto.Severity.Should().Be("critical");
        _ = dto.Description.Should().Be("Images must have alternative text");
    }

    [Fact]
    public void ResultPatchDto_WithNullValues_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new ResultPatchDto(
            WcagCriterion: null,
            Level: null,
            Severity: null,
            Description: null
        );

        // Assert
        _ = dto.Should().NotBeNull();
        _ = dto.WcagCriterion.Should().BeNull();
        _ = dto.Level.Should().BeNull();
        _ = dto.Severity.Should().BeNull();
        _ = dto.Description.Should().BeNull();
    }

    [Fact]
    public void AnalysisPatchDto_PropertyAccess_ShouldWork()
    {
        // Arrange
        var dto = new AnalysisPatchDto(
            ContentType: "application/json",
            ContentInput: "{}",
            SourceUrl: "https://test.com",
            ToolUsed: "lighthouse",
            Status: "in_progress",
            SummaryResult: "Running analysis",
            ResultJson: "{\"result\": \"pending\"}",
            DurationMs: 800,
            WcagVersion: "2.2",
            WcagLevel: "AAA",
            AxeViolations: 10,
            AxeNeedsReview: 5,
            AxeRecommendations: 15,
            AxePasses: 20,
            AxeIncomplete: 2,
            AxeInapplicable: 4,
            EaViolations: 8,
            EaNeedsReview: 3,
            EaRecommendations: 12,
            EaPasses: 18,
            EaIncomplete: 1,
            EaInapplicable: 3
        );

        // Act & Assert
        _ = dto.ContentType.Should().Be("application/json");
        _ = dto.ContentInput.Should().Be("{}");
        _ = dto.SourceUrl.Should().Be("https://test.com");
        _ = dto.ToolUsed.Should().Be("lighthouse");
        _ = dto.Status.Should().Be("in_progress");
        _ = dto.SummaryResult.Should().Be("Running analysis");
        _ = dto.ResultJson.Should().Be("{\"result\": \"pending\"}");
        _ = dto.DurationMs.Should().Be(800);
        _ = dto.WcagVersion.Should().Be("2.2");
        _ = dto.WcagLevel.Should().Be("AAA");
        _ = dto.AxeViolations.Should().Be(10);
        _ = dto.AxeNeedsReview.Should().Be(5);
        _ = dto.AxeRecommendations.Should().Be(15);
        _ = dto.AxePasses.Should().Be(20);
        _ = dto.AxeIncomplete.Should().Be(2);
        _ = dto.AxeInapplicable.Should().Be(4);
        _ = dto.EaViolations.Should().Be(8);
        _ = dto.EaNeedsReview.Should().Be(3);
        _ = dto.EaRecommendations.Should().Be(12);
        _ = dto.EaPasses.Should().Be(18);
        _ = dto.EaIncomplete.Should().Be(1);
        _ = dto.EaInapplicable.Should().Be(3);
    }

    [Fact]
    public void ErrorPatchDto_PropertyAccess_ShouldWork()
    {
        // Arrange
        var dto = new ErrorPatchDto(
            ErrorCode: "WCAG21_A_002",
            Description: "Form controls must have labels",
            Location: "input[type='text']:nth-child(2)"
        );

        // Act & Assert
        _ = dto.ErrorCode.Should().Be("WCAG21_A_002");
        _ = dto.Description.Should().Be("Form controls must have labels");
        _ = dto.Location.Should().Be("input[type='text']:nth-child(2)");
    }

    [Fact]
    public void ResultPatchDto_PropertyAccess_ShouldWork()
    {
        // Arrange
        var dto = new ResultPatchDto(
            WcagCriterion: "2.4.3 Focus Order",
            Level: "A",
            Severity: "moderate",
            Description: "Focusable elements must receive focus in logical order"
        );

        // Act & Assert
        _ = dto.WcagCriterion.Should().Be("2.4.3 Focus Order");
        _ = dto.Level.Should().Be("A");
        _ = dto.Severity.Should().Be("moderate");
        _ = dto.Description.Should().Be("Focusable elements must receive focus in logical order");
    }
}
