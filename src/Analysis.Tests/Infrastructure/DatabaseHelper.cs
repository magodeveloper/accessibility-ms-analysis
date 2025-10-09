using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Analysis.Tests.Infrastructure;

/// <summary>
/// Helper para manejo dinámico de bases de datos en tests
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// Asegura que la base de datos esté lista para tests
    /// </summary>
    public static async Task<bool> EnsureDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();

        try
        {
            // Verificar si la base de datos existe y tiene conexión
            var canConnect = await context.Database.CanConnectAsync();

            if (!canConnect)
            {
                Console.WriteLine("⚠️  No se puede conectar a la base de datos");
                return false;
            }

            // Verificar si hay migraciones pendientes
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                Console.WriteLine($"🔄 Aplicando {pendingMigrations.Count()} migraciones pendientes...");
                await context.Database.MigrateAsync();
                Console.WriteLine("✅ Migraciones aplicadas correctamente");
            }
            else
            {
                Console.WriteLine("✅ Base de datos actualizada, no hay migraciones pendientes");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error configurando base de datos: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Limpia datos de test manteniendo la estructura
    /// </summary>
    public static async Task CleanTestDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();

        try
        {
            // Limpiar en orden correcto para evitar problemas de FK
            var errors = await context.Errors.ToListAsync();
            context.Errors.RemoveRange(errors);

            var results = await context.Results.ToListAsync();
            context.Results.RemoveRange(results);

            var analyses = await context.Analyses.ToListAsync();
            context.Analyses.RemoveRange(analyses);

            _ = await context.SaveChangesAsync();
            Console.WriteLine("🧹 Datos de test limpiados correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error limpiando datos de test: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica el estado de la base de datos
    /// </summary>
    public static async Task<DatabaseStatus> GetDatabaseStatusAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();

        try
        {
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return DatabaseStatus.NotAccessible;
            }

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                return DatabaseStatus.MigrationsPending;
            }

            // Verificar si hay datos
            var hasData = await context.Analyses.AnyAsync();
            return hasData ? DatabaseStatus.ReadyWithData : DatabaseStatus.ReadyEmpty;
        }
        catch
        {
            return DatabaseStatus.NotAccessible;
        }
    }
}

public enum DatabaseStatus
{
    NotAccessible,
    MigrationsPending,
    ReadyEmpty,
    ReadyWithData
}
