using FluentAssertions;
using Analysis.Domain.Entities;

namespace Analysis.Tests.Domain;

public class EnumsTests
{
    [Fact]
    public void AnalysisStatus_ShouldHaveExpectedValues()
    {
        // Arrange
        string[] expected = ["pending", "success", "error"];

        // Assert
        Enum.GetNames<AnalysisStatus>().Should().Contain(expected);
    }

    [Fact]
    public void ContentType_ShouldHaveExpectedValues()
    {
        // Arrange
        string[] expected = ["html", "url"];

        // Assert
        Enum.GetNames<ContentType>().Should().Contain(expected);
    }

    [Fact]
    public void ToolUsed_ShouldHaveExpectedValues()
    {
        // Arrange
        string[] expected = ["axecore", "equalaccess"];

        // Assert
        Enum.GetNames<ToolUsed>().Should().Contain(expected);
    }

    [Fact]
    public void ResultLevel_ShouldHaveExpectedValues()
    {
        // Arrange
        string[] expected = ["violation", "recommendation"];

        // Assert
        Enum.GetNames<ResultLevel>().Should().Contain(expected);
    }

    [Fact]
    public void Severity_ShouldHaveExpectedValues()
    {
        // Arrange
        string[] expected = ["high", "medium", "low"];

        // Assert
        Enum.GetNames<Severity>().Should().Contain(expected);
    }

    [Fact]
    public void WcagLevel_ShouldHaveExpectedValues()
    {
        // Arrange
        string[] expected = ["A", "AA", "AAA"];

        // Assert
        Enum.GetNames<WcagLevel>().Should().Contain(expected);
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
        _ = result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ContentType.html, "html")]
    [InlineData(ContentType.url, "url")]
    public void ContentType_ToString_ShouldReturnCorrectStringValue(ContentType contentType, string expected)
    {
        // Act
        var result = contentType.ToString();

        // Assert
        _ = result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ToolUsed.axecore, "axecore")]
    [InlineData(ToolUsed.equalaccess, "equalaccess")]
    public void ToolUsed_ToString_ShouldReturnCorrectStringValue(ToolUsed toolUsed, string expected)
    {
        // Act
        var result = toolUsed.ToString();

        // Assert
        _ = result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ResultLevel.violation, "violation")]
    [InlineData(ResultLevel.recommendation, "recommendation")]
    public void ResultLevel_ToString_ShouldReturnCorrectStringValue(ResultLevel level, string expected)
    {
        // Act
        var result = level.ToString();

        // Assert
        _ = result.Should().Be(expected);
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
        _ = result.Should().Be(expected);
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
        _ = result.Should().Be(expected);
    }
}
