using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Analysis.Api.Helpers;
using Moq;

namespace Analysis.Tests.Helpers;

public class LanguageHelperTests
{
    [Fact]
    public void GetRequestLanguage_WithValidSpanishHeader_ShouldReturnEs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "es-ES,es;q=0.9,en;q=0.8";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithValidEnglishHeader_ShouldReturnEn()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "en-US,en;q=0.9,es;q=0.8";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public void GetRequestLanguage_WithEmptyHeader_ShouldReturnDefaultEs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithNullHeader_ShouldReturnDefaultEs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        // No se establece el header

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithWhitespaceHeader_ShouldReturnDefaultEs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "   ";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithTooShortLanguage_ShouldReturnDefaultEs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "x";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithUnsupportedLanguage_ShouldReturnDefaultEs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "fr-FR,fr;q=0.9";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("es");
    }

    [Fact]
    public void GetRequestLanguage_WithMultipleLanguagesEnglishFirst_ShouldReturnEn()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "en-GB,en;q=0.9,es-ES;q=0.8,es;q=0.7";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public void GetRequestLanguage_WithCaseInsensitive_ShouldReturnCorrectLanguage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "EN-US,EN;q=0.9";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public void GetRequestLanguage_WithSpacesAroundLanguage_ShouldReturnCorrectLanguage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = " en-US , en;q=0.9";

        // Act
        var result = LanguageHelper.GetRequestLanguage(context.Request);

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public void GetRequestLanguage_WhenExceptionThrown_ShouldReturnDefaultEs()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Headers)
            .Throws(new InvalidOperationException("Simulated exception"));

        // Act
        var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

        // Assert
        result.Should().Be("es");
    }
}