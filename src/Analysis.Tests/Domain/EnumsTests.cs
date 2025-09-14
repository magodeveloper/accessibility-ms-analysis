using Xunit;
using FluentAssertions;
using Analysis.Domain.Entities;

namespace Analysis.Tests.Domain;

public class EnumsTests
{
    [Fact]
    public void AnalysisStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(AnalysisStatus)).Should().Contain(new[] { "pending", "success", "error" });
    }

    [Fact]
    public void ContentType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(ContentType)).Should().Contain(new[] { "html", "url" });
    }

    [Fact]
    public void ToolUsed_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(ToolUsed)).Should().Contain(new[] { "axecore", "equalaccess" });
    }

    [Fact]
    public void ResultLevel_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(ResultLevel)).Should().Contain(new[] { "violation", "recommendation" });
    }

    [Fact]
    public void Severity_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(Severity)).Should().Contain(new[] { "high", "medium", "low" });
    }

    [Fact]
    public void WcagLevel_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(WcagLevel)).Should().Contain(new[] { "A", "AA", "AAA" });
    }

    [Theory]
    [InlineData(AnalysisStatus.pending, "pending")]
    [InlineData(AnalysisStatus.success, "success")]
    [InlineData(AnalysisStatus.error, "error")]
    public void AnalysisStatus_ToString_ShouldReturnCorrectStringValue(AnalysisStatus status, string expected)
    {
        // Act
        var result = status.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ContentType.html, "html")]
    [InlineData(ContentType.url, "url")]
    public void ContentType_ToString_ShouldReturnCorrectStringValue(ContentType contentType, string expected)
    {
        // Act
        var result = contentType.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ToolUsed.axecore, "axecore")]
    [InlineData(ToolUsed.equalaccess, "equalaccess")]
    public void ToolUsed_ToString_ShouldReturnCorrectStringValue(ToolUsed toolUsed, string expected)
    {
        // Act
        var result = toolUsed.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ResultLevel.violation, "violation")]
    [InlineData(ResultLevel.recommendation, "recommendation")]
    public void ResultLevel_ToString_ShouldReturnCorrectStringValue(ResultLevel level, string expected)
    {
        // Act
        var result = level.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Severity.high, "high")]
    [InlineData(Severity.medium, "medium")]
    [InlineData(Severity.low, "low")]
    public void Severity_ToString_ShouldReturnCorrectStringValue(Severity severity, string expected)
    {
        // Act
        var result = severity.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(WcagLevel.A, "A")]
    [InlineData(WcagLevel.AA, "AA")]
    [InlineData(WcagLevel.AAA, "AAA")]
    public void WcagLevel_ToString_ShouldReturnCorrectStringValue(WcagLevel level, string expected)
    {
        // Act
        var result = level.ToString();

        // Assert
        result.Should().Be(expected);
    }
}