using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Analysis.Api.HealthChecks;

/// <summary>
/// Health check personalizado para verificar la conectividad y estado de la base de datos MySQL
/// </summary>
public class DatabaseHealthCheck(AnalysisDbContext dbContext, ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    private readonly AnalysisDbContext _dbContext = dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger = logger;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar que la base de datos responda con una query simple
            // Para InMemory database, CanConnectAsync puede fallar con métodos relacionales
            bool canConnect;
            try
            {
                canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Si es InMemory database, simplemente verificamos que podemos hacer una query
                canConnect = true;
            }

            if (!canConnect)
            {
                _logger.LogWarning("Cannot connect to Analysis database");
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to the Analysis database",
                    data: new Dictionary<string, object>
                    {
                        { "database", "unknown" }
                    });
            }

            // Contar análisis como verificación adicional
            var analysisCount = await _dbContext.Analyses.CountAsync(cancellationToken);

            _logger.LogDebug("Database health check passed. Analysis count: {AnalysisCount}", analysisCount);

            return HealthCheckResult.Healthy(
                "Database is accessible and responsive",
                data: new Dictionary<string, object>
                {
                    { "analysisCount", analysisCount },
                    { "timestamp", DateTime.UtcNow }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");

            return HealthCheckResult.Unhealthy(
                "Database check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "timestamp", DateTime.UtcNow }
                });
        }
    }
}
