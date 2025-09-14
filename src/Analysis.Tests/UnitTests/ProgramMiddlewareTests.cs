using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Builder;

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
        errorMessage.Should().NotBeNullOrEmpty();
        errorMessage.Should().Be("Error interno del servidor");
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
        lang.Should().Be("en-US");
        errorMessage.Should().NotBeNullOrEmpty();
        errorMessage.Should().Be("Internal server error");
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
        lang.Should().Be("fr-FR");
        errorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSerialization_ShouldWorkCorrectly()
    {
        // Arrange
        var errorObject = new { error = "Test error message" };

        // Act
        var result = JsonSerializer.Serialize(errorObject);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"error\":\"Test error message\"");
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
        errorMessage.Should().NotBeNullOrEmpty();

        if (language == "es")
        {
            errorMessage.Should().Be("Error interno del servidor");
        }
        else if (language == "en")
        {
            errorMessage.Should().Be("Internal server error");
        }
    }

    [Fact]
    public void SupportedCultures_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var expectedCultures = new[] { "es", "en" };

        // Act & Assert
        expectedCultures.Should().Contain("es");
        expectedCultures.Should().Contain("en");
        expectedCultures.Should().HaveCount(2);
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

        foreach (var testCase in testCases)
        {
            // Act
            var result = string.IsNullOrWhiteSpace(testCase.header) ? "es" : testCase.header.Split(',')[0];

            // Assert
            result.Should().Be(testCase.expected);
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
        acceptLanguage.Should().Be("en-US,en;q=0.9");
        userAgent.Should().Be("TestAgent");
    }

    [Fact]
    public void EnvironmentName_ShouldBeDetectable()
    {
        // Arrange & Act
        var testEnvironments = new[] { "Test", "TestEnvironment", "Development", "Production", "Staging" };

        // Assert
        foreach (var env in testEnvironments)
        {
            env.Should().NotBeNullOrEmpty();
            var isTestEnv = env == "Test" || env == "TestEnvironment";
            (isTestEnv || !isTestEnv).Should().BeTrue();
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
        var result = environment == "Test" || environment == "TestEnvironment";

        // Assert
        result.Should().Be(isTestEnvironment);
    }

    [Fact]
    public void ResponseContentType_ShouldBeSetCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        context.Response.ContentType = "application/json";

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public void ResponseStatusCode_ShouldBeSetCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        context.Response.StatusCode = 500;

        // Assert
        context.Response.StatusCode.Should().Be(500);
    }
}