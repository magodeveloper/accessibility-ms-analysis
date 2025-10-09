using Moq;
using FluentAssertions;
using Analysis.Domain.Entities;
using Analysis.Api.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Analysis.Tests.HealthChecks;

/// <summary>
/// Tests unitarios para los Health Checks del microservicio Analysis
/// </summary>
public class HealthChecksTests
{
    #region ApplicationHealthCheck Tests

    [Fact]
    public async Task ApplicationHealthCheck_WhenApplicationIsRunning_ReturnsHealthy()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<ApplicationHealthCheck>>();
        var mockLifetime = new Mock<IHostApplicationLifetime>();

        // Crear un CancellationTokenSource que NO esté cancelado
        using var cts = new CancellationTokenSource();
        _ = mockLifetime.Setup(x => x.ApplicationStopping).Returns(cts.Token);

        var healthCheck = new ApplicationHealthCheck(mockLogger, mockLifetime.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Healthy);
        _ = result.Description.Should().Be("Application is running normally");
        _ = result.Data.Should().ContainKey("status");
        _ = result.Data["status"].Should().Be("running");
        _ = result.Data.Should().ContainKey("environment");
        _ = result.Data.Should().ContainKey("uptimeSeconds");
        _ = result.Data.Should().ContainKey("uptimeFormatted");
        _ = result.Data.Should().ContainKey("timestamp");
        _ = result.Data.Should().ContainKey("machineName");
        _ = result.Data.Should().ContainKey("processId");
    }

    [Fact]
    public async Task ApplicationHealthCheck_WhenApplicationIsStopping_ReturnsUnhealthy()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApplicationHealthCheck>>();
        var mockLifetime = new Mock<IHostApplicationLifetime>();

        // Crear un CancellationTokenSource que está cancelado (stopping)
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _ = mockLifetime.Setup(x => x.ApplicationStopping).Returns(cts.Token);

        var healthCheck = new ApplicationHealthCheck(mockLogger.Object, mockLifetime.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Unhealthy);
        _ = result.Description.Should().Be("Application is shutting down");
        _ = result.Data.Should().ContainKey("status");
        _ = result.Data["status"].Should().Be("stopping");
        _ = result.Data.Should().ContainKey("timestamp");

        // Verificar que se registró el warning
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Application is stopping")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ApplicationHealthCheck_WhenHealthy_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ApplicationHealthCheck>>();
        var mockLifetime = new Mock<IHostApplicationLifetime>();

        using var cts = new CancellationTokenSource();
        _ = mockLifetime.Setup(x => x.ApplicationStopping).Returns(cts.Token);

        var healthCheck = new ApplicationHealthCheck(mockLogger.Object, mockLifetime.Object);

        // Act
        _ = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert - Verificar que se registró un mensaje de debug
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Application health check passed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ApplicationHealthCheck_ReturnsUptimeInformation()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<ApplicationHealthCheck>>();
        var mockLifetime = new Mock<IHostApplicationLifetime>();

        using var cts = new CancellationTokenSource();
        _ = mockLifetime.Setup(x => x.ApplicationStopping).Returns(cts.Token);

        var healthCheck = new ApplicationHealthCheck(mockLogger, mockLifetime.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Data.Should().ContainKey("uptimeSeconds");
        _ = result.Data.Should().ContainKey("uptimeFormatted");

        var uptimeSeconds = result.Data["uptimeSeconds"];
        _ = uptimeSeconds.Should().BeOfType<double>();
        _ = ((double)uptimeSeconds).Should().BeGreaterThan(0);

        var uptimeFormatted = result.Data["uptimeFormatted"];
        _ = uptimeFormatted.Should().BeOfType<string>();
        _ = uptimeFormatted.ToString().Should().MatchRegex(@"\d+d \d+h \d+m");
    }

    #endregion

    #region DatabaseHealthCheck Tests

    [Fact]
    public async Task DatabaseHealthCheck_WhenDatabaseIsAccessible_ReturnsHealthy()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AnalysisDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);

        // Agregar algunos análisis de prueba
        context.Analyses.AddRange(
            new Analysis.Domain.Entities.Analysis
            {
                UserId = 1,
                DateAnalysis = DateTime.UtcNow,
                ContentType = ContentType.url,
                SourceUrl = "https://test1.com",
                ToolUsed = ToolUsed.axecore,
                Status = AnalysisStatus.success,
                SummaryResult = "Test 1",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.AA,
                Results = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Analysis.Domain.Entities.Analysis
            {
                UserId = 2,
                DateAnalysis = DateTime.UtcNow,
                ContentType = ContentType.html,
                ContentInput = "<html></html>",
                ToolUsed = ToolUsed.equalaccess,
                Status = AnalysisStatus.pending,
                SummaryResult = "Test 2",
                ResultJson = "{}",
                WcagVersion = "2.1",
                WcagLevel = WcagLevel.A,
                Results = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        _ = await context.SaveChangesAsync();

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Healthy);
        _ = result.Description.Should().Be("Database is accessible and responsive");
        _ = result.Data.Should().ContainKey("analysisCount");
        _ = result.Data["analysisCount"].Should().Be(2);
        _ = result.Data.Should().ContainKey("timestamp");
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenDatabaseIsEmpty_ReturnsHealthyWithZeroCount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AnalysisDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Healthy);
        _ = result.Data.Should().ContainKey("analysisCount");
        _ = result.Data["analysisCount"].Should().Be(0);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AnalysisDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), cts.Token);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenExceptionOccurs_ReturnsUnhealthy()
    {
        // Arrange - Usar un contexto que causará una excepción al intentar contar
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AnalysisDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();

        // Dispose del contexto para forzar una excepción cuando se intente usarlo
        await context.DisposeAsync();

        var healthCheck = new DatabaseHealthCheck(context, logger);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Unhealthy);
        _ = result.Description.Should().Be("Database check failed");
        _ = result.Data.Should().ContainKey("error");
        _ = result.Data.Should().ContainKey("timestamp");
        _ = result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AnalysisDbContext(options);
        var mockLogger = new Mock<ILogger<DatabaseHealthCheck>>();

        // Dispose del contexto para forzar una excepción
        await context.DisposeAsync();

        var healthCheck = new DatabaseHealthCheck(context, mockLogger.Object);

        // Act
        _ = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert - Verificar que se registró un error
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenHealthy_LogsDebugMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AnalysisDbContext(options);
        _ = context.Analyses.Add(new Analysis.Domain.Entities.Analysis
        {
            UserId = 1,
            DateAnalysis = DateTime.UtcNow,
            ContentType = ContentType.url,
            SourceUrl = "https://test.com",
            ToolUsed = ToolUsed.axecore,
            Status = AnalysisStatus.success,
            SummaryResult = "Test",
            ResultJson = "{}",
            WcagVersion = "2.1",
            WcagLevel = WcagLevel.AA,
            Results = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _ = await context.SaveChangesAsync();

        var mockLogger = new Mock<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, mockLogger.Object);

        // Act
        _ = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert - Verificar que se registró un mensaje de debug
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database health check passed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DatabaseHealthCheck_ReturnsTimestampInData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AnalysisDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);

        var beforeCheck = DateTime.UtcNow;

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        var afterCheck = DateTime.UtcNow;

        // Assert
        _ = result.Data.Should().ContainKey("timestamp");
        var timestamp = (DateTime)result.Data["timestamp"];
        _ = timestamp.Should().BeOnOrAfter(beforeCheck);
        _ = timestamp.Should().BeOnOrBefore(afterCheck);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenCannotConnect_ReturnsUnhealthy()
    {
        // Arrange - Crear un DbContext que no puede conectar
        var mockContext = new Mock<AnalysisDbContext>(
            new DbContextOptionsBuilder<AnalysisDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);

        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        _ = mockDatabase.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        _ = mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

        var mockLogger = new Mock<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(mockContext.Object, mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Unhealthy);
        _ = result.Description.Should().Be("Cannot connect to the Analysis database");
        _ = result.Data.Should().ContainKey("database");
        _ = result.Data["database"].Should().Be("unknown");

        // Verificar que se registró el warning
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cannot connect to Analysis database")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region MemoryHealthCheck Tests

    [Fact]
    public async Task MemoryHealthCheck_WhenMemoryIsNormal_ReturnsHealthy()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var threshold = 1024L * 1024L * 1024L; // 1GB
        var healthCheck = new MemoryHealthCheck(mockLogger, threshold);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Healthy);
        _ = result.Description.Should().Contain("Memory usage is normal");
        _ = result.Data.Should().ContainKey("allocatedMB");
        _ = result.Data.Should().ContainKey("thresholdMB");
        _ = result.Data.Should().ContainKey("gen0Collections");
        _ = result.Data.Should().ContainKey("gen1Collections");
        _ = result.Data.Should().ContainKey("gen2Collections");
    }

    [Fact]
    public async Task MemoryHealthCheck_WhenMemoryIsHigh_ReturnsDegraded()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MemoryHealthCheck>>();
        var currentMemory = GC.GetTotalMemory(forceFullCollection: false);
        var lowThreshold = currentMemory - 1; // Threshold menor que la memoria actual
        var healthCheck = new MemoryHealthCheck(mockLogger.Object, lowThreshold);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Status.Should().Be(HealthStatus.Degraded);
        _ = result.Description.Should().Contain("Memory usage is high");
        _ = result.Data.Should().ContainKey("allocatedMB");
        _ = result.Data.Should().ContainKey("thresholdMB");

        // Verificar que se registró el warning
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Memory usage is high")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task MemoryHealthCheck_WhenHealthy_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MemoryHealthCheck>>();
        var threshold = 1024L * 1024L * 1024L; // 1GB
        var healthCheck = new MemoryHealthCheck(mockLogger.Object, threshold);

        // Act
        _ = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert - Verificar que se registró un mensaje de debug
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Memory health check passed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task MemoryHealthCheck_ReturnsGarbageCollectionMetrics()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var threshold = 1024L * 1024L * 1024L;
        var healthCheck = new MemoryHealthCheck(mockLogger, threshold);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Data.Should().ContainKey("gen0Collections");
        _ = result.Data.Should().ContainKey("gen1Collections");
        _ = result.Data.Should().ContainKey("gen2Collections");

        var gen0 = result.Data["gen0Collections"];
        var gen1 = result.Data["gen1Collections"];
        var gen2 = result.Data["gen2Collections"];

        _ = gen0.Should().BeOfType<int>();
        _ = gen1.Should().BeOfType<int>();
        _ = gen2.Should().BeOfType<int>();

        _ = ((int)gen0).Should().BeGreaterOrEqualTo(0);
        _ = ((int)gen1).Should().BeGreaterOrEqualTo(0);
        _ = ((int)gen2).Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task MemoryHealthCheck_WithCustomThreshold_UsesProvidedValue()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var customThreshold = 512L * 1024L * 1024L; // 512MB
        var healthCheck = new MemoryHealthCheck(mockLogger, customThreshold);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        _ = result.Data.Should().ContainKey("thresholdMB");
        var thresholdMB = (double)result.Data["thresholdMB"];
        _ = thresholdMB.Should().Be(512.0);
    }

    #endregion
}
