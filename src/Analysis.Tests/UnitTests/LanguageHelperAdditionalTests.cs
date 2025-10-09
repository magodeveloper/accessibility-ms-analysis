using Moq;
using FluentAssertions;
using Analysis.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Analysis.Tests.UnitTests;

/// <summary>
/// Tests adicionales para mejorar la cobertura del LanguageHelper
/// </summary>
public class LanguageHelperAdditionalTests
{
    [Fact]
    public void GetRequestLanguage_WithEmptyAcceptLanguageHeader_ShouldReturnDefaultLanguage()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Accept-Language"] = new StringValues("")
        };
        _ = mockRequest.Setup(r => r.Headers).Returns(headers);

        // Act
        var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

        // Assert
        _ = result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithNullAcceptLanguageHeader_ShouldReturnDefaultLanguage()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        _ = mockRequest.Setup(r => r.Headers).Returns(headers);

        // Act
        var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

        // Assert
        _ = result.Should().Be("es");
    }

    [Theory]
    [InlineData("fr,en;q=0.9", "es")]     // fr no soportado, fallback a es
    [InlineData("pt-BR,pt;q=0.9", "es")]  // pt no soportado, fallback a es
    [InlineData("de,en-US;q=0.8", "es")]  // de no soportado, fallback a es
    [InlineData("it-IT,it;q=0.9,en;q=0.8", "es")] // it no soportado, fallback a es
    [InlineData("en-US,en;q=0.9", "en")]  // en soportado
    [InlineData("es-ES,es;q=0.9", "es")]  // es soportado
    public void GetRequestLanguage_WithVariousLanguages_ShouldReturnFirstLanguage(string acceptLanguage, string expectedLanguage)
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Accept-Language"] = new StringValues(acceptLanguage)
        };
        _ = mockRequest.Setup(r => r.Headers).Returns(headers);

        // Act
        var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

        // Assert
        _ = result.Should().Be(expectedLanguage);
    }

    [Fact]
    public void GetRequestLanguage_WithWhitespaceOnlyLanguage_ShouldReturnDefaultLanguage()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Accept-Language"] = new StringValues("   ")
        };
        _ = mockRequest.Setup(r => r.Headers).Returns(headers);

        // Act
        var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

        // Assert
        _ = result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithComplexAcceptLanguageString_ShouldParseCorrectly()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Accept-Language"] = new StringValues("zh-CN,zh;q=0.9,en;q=0.8,es;q=0.7")
        };
        _ = mockRequest.Setup(r => r.Headers).Returns(headers);

        // Act
        var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

        // Assert
        _ = result.Should().Be("es"); // zh no soportado, fallback a es
    }
}
