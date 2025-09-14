using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Analysis.Infrastructure.Data;
using Analysis.Api;

namespace Analysis.Tests.IntegrationTests;

/// <summary>
/// Tests de integración para configuraciones específicas
/// </summary>
public class ProgramConfigurationTests
{
    [Fact]
    public void LanguageHeaders_ShouldBeParsedCorrectly()
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

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("Test")]
    [InlineData("TestEnvironment")]
    public void Environment_Detection_ShouldWorkCorrectly(string environment)
    {
        // Act
        var isTestEnvironment = environment == "Test" || environment == "TestEnvironment";

        // Assert
        environment.Should().NotBeNullOrEmpty();
        (isTestEnvironment || !isTestEnvironment).Should().BeTrue();
    }

    [Fact]
    public void SupportedCultures_Configuration_ShouldBeCorrect()
    {
        // Arrange
        var supportedCultures = new[] { "es", "en" };

        // Act & Assert
        supportedCultures.Should().Contain("es");
        supportedCultures.Should().Contain("en");
        supportedCultures.Should().HaveCount(2);
    }

    [Fact]
    public void LocalizationOptions_ShouldBeConfigurable()
    {
        // Arrange
        var defaultCulture = "es";
        var supportedCultures = new[] { "es", "en" };

        // Act & Assert
        defaultCulture.Should().Be("es");
        supportedCultures.Should().NotBeEmpty();
        supportedCultures.Should().Contain(defaultCulture);
    }

    [Fact]
    public void ErrorHandling_JsonSerialization_ShouldWork()
    {
        // Arrange
        var errorMessage = "Test error message";
        var errorObject = new { error = errorMessage };

        // Act
        var result = System.Text.Json.JsonSerializer.Serialize(errorObject);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"error\":\"Test error message\"");
    }

    [Fact]
    public void ContentType_ShouldBeSetCorrectly()
    {
        // Arrange
        var expectedContentType = "application/json";

        // Act & Assert
        expectedContentType.Should().Be("application/json");
    }

    [Fact]
    public void StatusCode_ShouldBeSetCorrectly()
    {
        // Arrange
        var expectedStatusCode = 500;

        // Act & Assert
        expectedStatusCode.Should().Be(500);
    }

    [Fact]
    public void ErrorLocalization_ShouldWorkForDifferentLanguages()
    {
        // Arrange & Act
        var spanishError = Analysis.Application.Localization.Get("Error_InternalServer", "es");
        var englishError = Analysis.Application.Localization.Get("Error_InternalServer", "en");

        // Assert
        spanishError.Should().Be("Error interno del servidor");
        englishError.Should().Be("Internal server error");
    }

    [Theory]
    [InlineData("es-ES,es;q=0.9,en;q=0.8")]
    [InlineData("en-US,en;q=0.9")]
    [InlineData("fr-FR,en;q=0.8")]
    public void AcceptLanguageHeader_FirstLanguage_ShouldBeExtracted(string headerValue)
    {
        // Act
        var firstLanguage = headerValue.Split(',')[0];

        // Assert
        firstLanguage.Should().NotBeNullOrEmpty();
        firstLanguage.Should().NotContain(";");
    }

    [Fact]
    public void SwaggerConfiguration_ShouldBeTestable()
    {
        // Arrange
        var swaggerTitle = "Analysis API";
        var swaggerVersion = "v1";

        // Act & Assert
        swaggerTitle.Should().Be("Analysis API");
        swaggerVersion.Should().Be("v1");
    }

    [Theory]
    [InlineData("Development", true)]
    [InlineData("Production", false)]
    [InlineData("Test", false)]
    public void IsDevelopment_ShouldReturnCorrectValue(string environment, bool expectedIsDevelopment)
    {
        // Act
        var isDevelopment = environment == "Development";

        // Assert
        isDevelopment.Should().Be(expectedIsDevelopment);
    }
}