using Moq;
using System.Net;
using Analysis.Api;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Analysis.Tests.Infrastructure;

public class MiddlewareTests(TestWebApplicationFactory<Program> factory) : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory = factory;

    [Theory]
    [InlineData("es", "Error interno del servidor")]
    [InlineData("en", "Internal server error")]
    [InlineData("fr", "Internal server error")] // Fallback to English (messages.en.json)
    public async Task ExceptionHandler_WithDifferentLanguages_ShouldReturnLocalizedError(string language, string expectedMessage)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            _ = builder.ConfigureServices(services =>
            {
                // Reemplazar el servicio de análisis con uno que lance excepción
                var descriptor = services.Single(d => d.ServiceType == typeof(Analysis.Application.Services.Analysis.IAnalysisService));
                _ = services.Remove(descriptor);

                var mockService = new Mock<Analysis.Application.Services.Analysis.IAnalysisService>();
                _ = mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new InvalidOperationException("Test exception"));

                _ = services.AddSingleton(mockService.Object);
            });
        }).CreateClient();

        // Agregar headers de autenticación (X-Gateway-Secret y X-User-*)
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", "test-gateway-secret-key");
        client.DefaultRequestHeaders.Add("X-User-Id", "1");
        client.DefaultRequestHeaders.Add("X-User-Email", "test@example.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "admin");
        client.DefaultRequestHeaders.Add("X-User-Name", "Test User");
        client.DefaultRequestHeaders.Add("Accept-Language", language);

        // Act
        var response = await client.GetAsync("/api/analysis/1");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        _ = (response.Content.Headers.ContentType?.MediaType.Should().Be("application/json"));

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(content);

        _ = jsonResponse.RootElement.GetProperty("error").GetString().Should().Be(expectedMessage);
    }

    [Fact]
    public async Task ExceptionHandler_WithNoAcceptLanguageHeader_ShouldReturnDefaultSpanishError()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            _ = builder.ConfigureServices(services =>
            {
                // Reemplazar el servicio de análisis con uno que lance excepción
                var descriptor = services.Single(d => d.ServiceType == typeof(Analysis.Application.Services.Analysis.IAnalysisService));
                _ = services.Remove(descriptor);

                var mockService = new Mock<Analysis.Application.Services.Analysis.IAnalysisService>();
                _ = mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new InvalidOperationException("Test exception"));

                _ = services.AddSingleton(mockService.Object);
            });
        }).CreateClient();

        // Agregar headers de autenticación pero NO Accept-Language
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", "test-gateway-secret-key");
        client.DefaultRequestHeaders.Add("X-User-Id", "1");
        client.DefaultRequestHeaders.Add("X-User-Email", "test@example.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "admin");
        client.DefaultRequestHeaders.Add("X-User-Name", "Test User");

        // Act
        var response = await client.GetAsync("/api/analysis/1");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(content);

        _ = jsonResponse.RootElement.GetProperty("error").GetString().Should().Be("Error interno del servidor");
    }

    [Fact]
    public async Task ExceptionHandler_WithComplexAcceptLanguageHeader_ShouldParseCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            _ = builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(d => d.ServiceType == typeof(Analysis.Application.Services.Analysis.IAnalysisService));
                _ = services.Remove(descriptor);

                var mockService = new Mock<Analysis.Application.Services.Analysis.IAnalysisService>();
                _ = mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new InvalidOperationException("Test exception"));

                _ = services.AddSingleton(mockService.Object);
            });
        }).CreateClient();

        // Agregar headers de autenticación y Accept-Language complejo
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", "test-gateway-secret-key");
        client.DefaultRequestHeaders.Add("X-User-Id", "1");
        client.DefaultRequestHeaders.Add("X-User-Email", "test@example.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "admin");
        client.DefaultRequestHeaders.Add("X-User-Name", "Test User");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,es;q=0.8");

        // Act
        var response = await client.GetAsync("/api/analysis/1");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(content);

        // Debería tomar "en-US" y devolver el mensaje en inglés
        _ = jsonResponse.RootElement.GetProperty("error").GetString().Should().Be("Internal server error");
    }
}
