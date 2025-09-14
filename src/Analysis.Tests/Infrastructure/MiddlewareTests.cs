using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Analysis.Api;

namespace Analysis.Tests.Infrastructure;

public class MiddlewareTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public MiddlewareTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("es", "Error interno del servidor")]
    [InlineData("en", "Internal server error")]
    [InlineData("fr", "Internal server error")] // Fallback to English (messages.en.json)
    public async Task ExceptionHandler_WithDifferentLanguages_ShouldReturnLocalizedError(string language, string expectedMessage)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Reemplazar el servicio de análisis con uno que lance excepción
                var descriptor = services.Single(d => d.ServiceType == typeof(Analysis.Application.Services.Analysis.IAnalysisService));
                services.Remove(descriptor);

                var mockService = new Mock<Analysis.Application.Services.Analysis.IAnalysisService>();
                mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new InvalidOperationException("Test exception"));

                services.AddSingleton(mockService.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Add("Accept-Language", language);

        // Act
        var response = await client.GetAsync("/api/analysis/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(content);

        jsonResponse.RootElement.GetProperty("error").GetString().Should().Be(expectedMessage);
    }

    [Fact]
    public async Task ExceptionHandler_WithNoAcceptLanguageHeader_ShouldReturnDefaultSpanishError()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Reemplazar el servicio de análisis con uno que lance excepción
                var descriptor = services.Single(d => d.ServiceType == typeof(Analysis.Application.Services.Analysis.IAnalysisService));
                services.Remove(descriptor);

                var mockService = new Mock<Analysis.Application.Services.Analysis.IAnalysisService>();
                mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new InvalidOperationException("Test exception"));

                services.AddSingleton(mockService.Object);
            });
        }).CreateClient();

        // No agregar Accept-Language header

        // Act
        var response = await client.GetAsync("/api/analysis/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(content);

        jsonResponse.RootElement.GetProperty("error").GetString().Should().Be("Error interno del servidor");
    }

    [Fact]
    public async Task ExceptionHandler_WithComplexAcceptLanguageHeader_ShouldParseCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(d => d.ServiceType == typeof(Analysis.Application.Services.Analysis.IAnalysisService));
                services.Remove(descriptor);

                var mockService = new Mock<Analysis.Application.Services.Analysis.IAnalysisService>();
                mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new InvalidOperationException("Test exception"));

                services.AddSingleton(mockService.Object);
            });
        }).CreateClient();

        // Simular un header complejo como "en-US,en;q=0.9,es;q=0.8"
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,es;q=0.8");

        // Act
        var response = await client.GetAsync("/api/analysis/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(content);

        // Debería tomar "en-US" y devolver el mensaje en inglés
        jsonResponse.RootElement.GetProperty("error").GetString().Should().Be("Internal server error");
    }
}