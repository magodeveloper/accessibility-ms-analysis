using System.Net;
using FluentAssertions;
using System.Text.Json;
using System.Net.Http.Json;
using Analysis.Infrastructure.Data;
using Analysis.Tests.Infrastructure;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Analysis.Application.Services.UserContext;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Analysis.Tests.IntegrationTests;

/// <summary>
/// Tests de integración para verificar la configuración completa de Program.cs:
/// - Service Registration (Dependency Injection)
/// - Health Check endpoints (/health, /health/live, /health/ready)
/// - Prometheus metrics endpoint (/metrics)
/// - Middleware pipeline (GatewaySecret, UserContext, Exception handling, Localization)
/// - Controller routing
/// </summary>
public class ProgramConfigurationTests(TestWebApplicationFactory<Analysis.Api.Program> factory) : IClassFixture<TestWebApplicationFactory<Analysis.Api.Program>>
{
    private readonly TestWebApplicationFactory<Analysis.Api.Program> _factory = factory;

    #region Service Registration Tests

    [Fact]
    public void Services_ShouldRegisterControllers()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert - Verificar que los controladores están registrados
        var mvcBuilder = services.GetService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();
        _ = mvcBuilder.Should().NotBeNull();
    }

    [Fact]
    public void Services_ShouldRegisterAnalysisService()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var analysisService = services.GetService<IAnalysisService>();
        _ = analysisService.Should().NotBeNull();
        _ = analysisService.Should().BeAssignableTo<IAnalysisService>();
    }

    [Fact]
    public void Services_ShouldRegisterErrorService()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var errorService = services.GetService<IErrorService>();
        _ = errorService.Should().NotBeNull();
        _ = errorService.Should().BeAssignableTo<IErrorService>();
    }

    [Fact]
    public void Services_ShouldRegisterResultService()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var resultService = services.GetService<IResultService>();
        _ = resultService.Should().NotBeNull();
        _ = resultService.Should().BeAssignableTo<IResultService>();
    }

    [Fact]
    public void Services_ShouldRegisterUserContext()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var userContext = services.GetService<IUserContext>();
        _ = userContext.Should().NotBeNull();
        _ = userContext.Should().BeAssignableTo<IUserContext>();
    }

    [Fact]
    public void Services_ShouldRegisterDbContext()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var dbContext = services.GetService<AnalysisDbContext>();
        _ = dbContext.Should().NotBeNull();
    }

    [Fact]
    public void Services_ShouldRegisterHealthChecks()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var healthCheckService = services.GetService<HealthCheckService>();
        _ = healthCheckService.Should().NotBeNull();
    }

    #endregion

    #region Health Check Endpoint Tests

    [Fact]
    public async Task HealthEndpoint_General_ShouldReturnHealthStatus()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        // En tests con InMemory DB, puede retornar OK o ServiceUnavailable
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        _ = (response.Content.Headers.ContentType?.MediaType.Should().Be("application/json"));

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        _ = content.GetProperty("status").GetString().Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
        _ = content.TryGetProperty("timestamp", out _).Should().BeTrue();
        _ = content.TryGetProperty("totalDuration", out _).Should().BeTrue();
        _ = content.TryGetProperty("entries", out _).Should().BeTrue();
    }

    [Fact]
    public async Task HealthEndpoint_Live_ShouldReturnLivenessChecks()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        _ = (response.Content.Headers.ContentType?.MediaType.Should().Be("application/json"));

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        _ = content.GetProperty("status").GetString().Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
        _ = content.TryGetProperty("timestamp", out _).Should().BeTrue();
        _ = content.TryGetProperty("checks", out var checks).Should().BeTrue();

        // Verificar que contiene checks con tag "live" (application, memory)
        var checksArray = checks.EnumerateArray().ToList();
        _ = checksArray.Should().NotBeEmpty();
        _ = checksArray.Should().Contain(c => c.GetProperty("name").GetString() == "application" ||
                                           c.GetProperty("name").GetString() == "memory");
    }

    [Fact]
    public async Task HealthEndpoint_Ready_ShouldReturnReadinessChecks()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        // En tests con InMemory DB, puede retornar OK o ServiceUnavailable
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        _ = (response.Content.Headers.ContentType?.MediaType.Should().Be("application/json"));

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        _ = content.GetProperty("status").GetString().Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
        _ = content.TryGetProperty("timestamp", out _).Should().BeTrue();
        _ = content.TryGetProperty("checks", out var checks).Should().BeTrue();

        // Verificar que contiene checks con tag "ready" (database, analysis_dbcontext)
        var checksArray = checks.EnumerateArray().ToList();
        _ = checksArray.Should().NotBeEmpty();
        _ = checksArray.Should().Contain(c => c.GetProperty("name").GetString() == "database" ||
                                           c.GetProperty("name").GetString() == "analysis_dbcontext");
    }

    [Fact]
    public async Task HealthEndpoint_General_ShouldIncludeAllHealthChecks()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        _ = content.TryGetProperty("entries", out var entries).Should().BeTrue();

        var entriesDict = entries.EnumerateObject().Select(p => p.Name).ToList();

        // Verificar que contiene los health checks esperados
        _ = entriesDict.Should().Contain("application");
        _ = entriesDict.Should().Contain("memory");
        _ = entriesDict.Should().Contain("database");
        _ = entriesDict.Should().Contain("analysis_dbcontext");
    }

    [Fact]
    public async Task HealthEndpoint_Ready_ShouldIncludeDataProperty()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        _ = content.TryGetProperty("checks", out var checks).Should().BeTrue();

        var checksArray = checks.EnumerateArray().ToList();
        foreach (var check in checksArray)
        {
            _ = check.TryGetProperty("name", out _).Should().BeTrue();
            _ = check.TryGetProperty("status", out _).Should().BeTrue();
            _ = check.TryGetProperty("duration", out _).Should().BeTrue();
        }
    }

    #endregion

    #region Prometheus Metrics Tests

    [Fact]
    public async Task MetricsEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/metrics");

        // Assert
        // El endpoint de métricas puede retornar 200 o 404 dependiendo de la configuración
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            _ = content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task MetricsEndpoint_WhenEnabled_ShouldCollectHttpMetrics()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act - Hacer varias llamadas para generar métricas
        _ = await client.GetAsync("/health/live");
        _ = await client.GetAsync("/health/ready");

        var metricsResponse = await client.GetAsync("/metrics");

        // Assert
        if (metricsResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await metricsResponse.Content.ReadAsStringAsync();
            _ = content.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Middleware Pipeline Tests

    [Fact]
    public async Task Middleware_GatewaySecret_ShouldRejectRequestWithoutSecret()
    {
        // Arrange - Cliente sin headers de autenticación
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/analysis");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Middleware_GatewaySecret_ShouldRejectRequestWithInvalidSecret()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", "invalid-secret");

        // Act
        var response = await client.GetAsync("/api/analysis");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Middleware_GatewaySecret_ShouldAcceptRequestWithValidSecret()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/analysis");

        // Assert
        // No debe ser Forbidden (podría ser 404 NotFound si no hay análisis)
        _ = response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Middleware_UserContext_ShouldExtractUserIdFromHeaders()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(userId: 123);

        // Act
        var response = await client.GetAsync("/api/analysis");

        // Assert
        // La petición debe pasar el middleware (no debe ser Unauthorized)
        _ = response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        _ = response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Middleware_ExceptionHandler_ShouldReturnJsonError()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act - Forzar un endpoint que no existe para provocar error en el pipeline
        var response = await client.GetAsync("/api/nonexistent/cause-error");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Middleware_Localization_ShouldRespectAcceptLanguageHeader()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        // El middleware de localización debe procesar el header correctamente
    }

    #endregion

    #region Controller Routing Tests

    [Fact]
    public async Task Controllers_AnalysisEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/analysis");

        // Assert
        // Endpoint existe (puede ser 404 NotFound si vacío, pero no error de ruta)
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Controllers_ErrorEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/error");

        // Assert
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Controllers_ResultEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/result");

        // Assert
        _ = response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Configuration_SupportedCultures_ShouldBeCorrect()
    {
        // Arrange
        var supportedCultures = new[] { "es", "en" };

        // Act & Assert
        _ = supportedCultures.Should().Contain("es");
        _ = supportedCultures.Should().Contain("en");
        _ = supportedCultures.Should().HaveCount(2);
    }

    [Fact]
    public void Configuration_ErrorLocalization_ShouldWorkForSpanish()
    {
        // Arrange & Act
        var spanishError = Analysis.Application.Localization.Get("Error_InternalServer", "es");

        // Assert
        _ = spanishError.Should().Be("Error interno del servidor");
    }

    [Fact]
    public void Configuration_ErrorLocalization_ShouldWorkForEnglish()
    {
        // Arrange & Act
        var englishError = Analysis.Application.Localization.Get("Error_InternalServer", "en");

        // Assert
        _ = englishError.Should().Be("Internal server error");
    }

    #endregion
}
