using Analysis.Application.Dtos;
using FluentAssertions;
using Xunit;

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
        dto.Should().NotBeNull();
        dto.ContentType.Should().Be("text/html");
        dto.ContentInput.Should().Be("<html><body>Test content</body></html>");
        dto.SourceUrl.Should().Be("https://example.com");
        dto.ToolUsed.Should().Be("axe-core");
        dto.Status.Should().Be("completed");
        dto.SummaryResult.Should().Be("Analysis completed successfully");
        dto.ResultJson.Should().Be("{}");
        dto.DurationMs.Should().Be(1500);
        dto.WcagVersion.Should().Be("2.1");
        dto.WcagLevel.Should().Be("AA");
        dto.AxeViolations.Should().Be(5);
        dto.AxeNeedsReview.Should().Be(2);
        dto.AxeRecommendations.Should().Be(8);
        dto.AxePasses.Should().Be(15);
        dto.AxeIncomplete.Should().Be(1);
        dto.AxeInapplicable.Should().Be(3);
        dto.EaViolations.Should().Be(4);
        dto.EaNeedsReview.Should().Be(1);
        dto.EaRecommendations.Should().Be(6);
        dto.EaPasses.Should().Be(12);
        dto.EaIncomplete.Should().Be(0);
        dto.EaInapplicable.Should().Be(2);
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
        dto.Should().NotBeNull();
        dto.ContentType.Should().BeNull();
        dto.ContentInput.Should().BeNull();
        dto.SourceUrl.Should().BeNull();
        dto.ToolUsed.Should().BeNull();
        dto.Status.Should().BeNull();
        dto.SummaryResult.Should().BeNull();
        dto.ResultJson.Should().BeNull();
        dto.DurationMs.Should().BeNull();
        dto.WcagVersion.Should().BeNull();
        dto.WcagLevel.Should().BeNull();
        dto.AxeViolations.Should().BeNull();
        dto.AxeNeedsReview.Should().BeNull();
        dto.AxeRecommendations.Should().BeNull();
        dto.AxePasses.Should().BeNull();
        dto.AxeIncomplete.Should().BeNull();
        dto.AxeInapplicable.Should().BeNull();
        dto.EaViolations.Should().BeNull();
        dto.EaNeedsReview.Should().BeNull();
        dto.EaRecommendations.Should().BeNull();
        dto.EaPasses.Should().BeNull();
        dto.EaIncomplete.Should().BeNull();
        dto.EaInapplicable.Should().BeNull();
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
        dto.Should().NotBeNull();
        dto.ErrorCode.Should().Be("WCAG21_AA_001");
        dto.Description.Should().Be("Missing alt text for image");
        dto.Location.Should().Be("element[img]:nth-child(1)");
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
        dto.Should().NotBeNull();
        dto.ErrorCode.Should().BeNull();
        dto.Description.Should().BeNull();
        dto.Location.Should().BeNull();
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
        dto.Should().NotBeNull();
        dto.WcagCriterion.Should().Be("1.1.1 Non-text Content");
        dto.Level.Should().Be("A");
        dto.Severity.Should().Be("critical");
        dto.Description.Should().Be("Images must have alternative text");
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
        dto.Should().NotBeNull();
        dto.WcagCriterion.Should().BeNull();
        dto.Level.Should().BeNull();
        dto.Severity.Should().BeNull();
        dto.Description.Should().BeNull();
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
        dto.ContentType.Should().Be("application/json");
        dto.ContentInput.Should().Be("{}");
        dto.SourceUrl.Should().Be("https://test.com");
        dto.ToolUsed.Should().Be("lighthouse");
        dto.Status.Should().Be("in_progress");
        dto.SummaryResult.Should().Be("Running analysis");
        dto.ResultJson.Should().Be("{\"result\": \"pending\"}");
        dto.DurationMs.Should().Be(800);
        dto.WcagVersion.Should().Be("2.2");
        dto.WcagLevel.Should().Be("AAA");
        dto.AxeViolations.Should().Be(10);
        dto.AxeNeedsReview.Should().Be(5);
        dto.AxeRecommendations.Should().Be(15);
        dto.AxePasses.Should().Be(20);
        dto.AxeIncomplete.Should().Be(2);
        dto.AxeInapplicable.Should().Be(4);
        dto.EaViolations.Should().Be(8);
        dto.EaNeedsReview.Should().Be(3);
        dto.EaRecommendations.Should().Be(12);
        dto.EaPasses.Should().Be(18);
        dto.EaIncomplete.Should().Be(1);
        dto.EaInapplicable.Should().Be(3);
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
        dto.ErrorCode.Should().Be("WCAG21_A_002");
        dto.Description.Should().Be("Form controls must have labels");
        dto.Location.Should().Be("input[type='text']:nth-child(2)");
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
        dto.WcagCriterion.Should().Be("2.4.3 Focus Order");
        dto.Level.Should().Be("A");
        dto.Severity.Should().Be("moderate");
        dto.Description.Should().Be("Focusable elements must receive focus in logical order");
    }
}