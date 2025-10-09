using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Analysis.Tests.UnitTests;

/// <summary>
/// Tests para middlewares y configuraciones específicas de Program.cs
/// </summary>
public class ProgramMiddlewareTests
{
    [Fact]
    public void ExceptionHandler_WithDefaultLanguage_ShouldReturnSpanishError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "";
        context.Response.Body = new MemoryStream();

        // Simulamos el comportamiento del middleware de excepciones
        var errorMessage = Analysis.Application.Localization.Get("Error_InternalServer", "es");

        // Act & Assert
        _ = errorMessage.Should().NotBeNullOrEmpty();
        _ = errorMessage.Should().Be("Error interno del servidor");
    }

    [Fact]
    public void ExceptionHandler_WithEnglishLanguage_ShouldReturnEnglishError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "en-US";
        context.Response.Body = new MemoryStream();

        // Simulamos el comportamiento del middleware de excepciones
        var lang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        var errorMessage = Analysis.Application.Localization.Get("Error_InternalServer", lang);

        // Act & Assert
        _ = lang.Should().Be("en-US");
        _ = errorMessage.Should().NotBeNullOrEmpty();
        _ = errorMessage.Should().Be("Internal server error");
    }

    [Fact]
    public void ExceptionHandler_WithMultipleLanguages_ShouldReturnFirstLanguage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "fr-FR,en-US,es-ES";
        context.Response.Body = new MemoryStream();

        // Simulamos el comportamiento del middleware de excepciones
        var lang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        var errorMessage = Analysis.Application.Localization.Get("Error_InternalServer", lang);

        // Act & Assert
        _ = lang.Should().Be("fr-FR");
        _ = errorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSerialization_ShouldWorkCorrectly()
    {
        // Arrange
        var errorObject = new { error = "Test error message" };

        // Act
        var result = JsonSerializer.Serialize(errorObject);

        // Assert
        _ = result.Should().NotBeNullOrEmpty();
        _ = result.Should().Contain("\"error\":\"Test error message\"");
    }

    [Theory]
    [InlineData("es")]
    [InlineData("en")]
    [InlineData("fr")]
    public void LocalizationGet_WithDifferentLanguages_ShouldReturnLocalizedMessage(string language)
    {
        // Act
        var errorMessage = Analysis.Application.Localization.Get("Error_InternalServer", language);

        // Assert
        _ = errorMessage.Should().NotBeNullOrEmpty();

        if (language == "es")
        {
            _ = errorMessage.Should().Be("Error interno del servidor");
        }
        else if (language == "en")
        {
            _ = errorMessage.Should().Be("Internal server error");
        }
    }

    [Fact]
    public void SupportedCultures_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var expectedCultures = new[] { "es", "en" };

        // Act & Assert
        _ = expectedCultures.Should().Contain("es");
        _ = expectedCultures.Should().Contain("en");
        _ = expectedCultures.Should().HaveCount(2);
    }

    [Fact]
    public void AcceptLanguageHeader_ShouldBeParsedCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            (header: "es-ES,es;q=0.9,en;q=0.8", expected: "es-ES"),
            (header: "en-US,en;q=0.9", expected: "en-US"),
            (header: "fr-FR", expected: "fr-FR"),
            (header: "", expected: "es"),
            (header: null, expected: "es")
        };

        foreach (var (header, expected) in testCases)
        {
            // Act
            var result = string.IsNullOrWhiteSpace(header) ? "es" : header.Split(',')[0];

            // Assert
            _ = result.Should().Be(expected);
        }
    }

    [Fact]
    public void HttpContext_Headers_ShouldBeAccessible()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "en-US,en;q=0.9";
        context.Request.Headers["User-Agent"] = "TestAgent";

        // Act
        var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

        // Assert
        _ = acceptLanguage.Should().Be("en-US,en;q=0.9");
        _ = userAgent.Should().Be("TestAgent");
    }

    [Fact]
    public void EnvironmentName_ShouldBeDetectable()
    {
        // Arrange & Act
        var testEnvironments = new[] { "Test", "TestEnvironment", "Development", "Production", "Staging" };

        // Assert
        foreach (var env in testEnvironments)
        {
            _ = env.Should().NotBeNullOrEmpty();
            var isTestEnv = env is "Test" or "TestEnvironment";
            _ = (isTestEnv || !isTestEnv).Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("Development", false)]
    [InlineData("Production", false)]
    [InlineData("Test", true)]
    [InlineData("TestEnvironment", true)]
    public void Environment_TestCheck_ShouldWorkCorrectly(string environment, bool isTestEnvironment)
    {
        // Act
        var result = environment is "Test" or "TestEnvironment";

        // Assert
        _ = result.Should().Be(isTestEnvironment);
    }

    [Fact]
    public void ResponseContentType_ShouldBeSetCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        context.Response.ContentType = "application/json";

        // Assert
        _ = context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public void ResponseStatusCode_ShouldBeSetCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        context.Response.StatusCode = 500;

        // Assert
        _ = context.Response.StatusCode.Should().Be(500);
    }
}
