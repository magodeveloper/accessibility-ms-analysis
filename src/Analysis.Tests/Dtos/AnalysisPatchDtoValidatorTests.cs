using Analysis.Application.Dtos;
using FluentAssertions;
using Xunit;

namespace Analysis.Tests.Dtos;

public class AnalysisPatchDtoValidatorTests
{
    private readonly AnalysisPatchDtoValidator _validator = new();

    [Theory]
    [InlineData("2.0")]
    [InlineData("2.1")]
    [InlineData("2.2")]
    [InlineData(null)]
    public void Validate_WcagVersion_ValidValues_ShouldPass(string? wcagVersion)
    {
        // Arrange
        var dto = new AnalysisPatchDto(
            ContentType: null,
            ContentInput: null,
            SourceUrl: null,
            ToolUsed: null,
            Status: null,
            SummaryResult: null,
            ResultJson: null,
            DurationMs: null,
            WcagVersion: wcagVersion,
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

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AA")]
    [InlineData("AAA")]
    [InlineData(null)]
    public void Validate_WcagLevel_ValidValues_ShouldPass(string? wcagLevel)
    {
        // Arrange
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
            WcagLevel: wcagLevel,
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

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("3.0")]
    [InlineData("invalid")]
    [InlineData("")]
    public void Validate_WcagVersion_InvalidValues_ShouldFail(string wcagVersion)
    {
        // Arrange
        var dto = new AnalysisPatchDto(
            ContentType: null,
            ContentInput: null,
            SourceUrl: null,
            ToolUsed: null,
            Status: null,
            SummaryResult: null,
            ResultJson: null,
            DurationMs: null,
            WcagVersion: wcagVersion,
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

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WcagVersion");
    }

    [Theory]
    [InlineData("B")]
    [InlineData("AAAA")]
    [InlineData("invalid")]
    [InlineData("")]
    public void Validate_WcagLevel_InvalidValues_ShouldFail(string wcagLevel)
    {
        // Arrange
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
            WcagLevel: wcagLevel,
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

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WcagLevel");
    }

    [Fact]
    public void Validate_BothValidValues_ShouldPass()
    {
        // Arrange
        var dto = new AnalysisPatchDto(
            ContentType: null,
            ContentInput: null,
            SourceUrl: null,
            ToolUsed: null,
            Status: null,
            SummaryResult: null,
            ResultJson: null,
            DurationMs: null,
            WcagVersion: "2.1",
            WcagLevel: "AA",
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

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_BothInvalidValues_ShouldFail()
    {
        // Arrange
        var dto = new AnalysisPatchDto(
            ContentType: null,
            ContentInput: null,
            SourceUrl: null,
            ToolUsed: null,
            Status: null,
            SummaryResult: null,
            ResultJson: null,
            DurationMs: null,
            WcagVersion: "invalid",
            WcagLevel: "invalid",
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

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WcagVersion");
        result.Errors.Should().Contain(e => e.PropertyName == "WcagLevel");
    }
}