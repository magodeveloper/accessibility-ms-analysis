using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Analysis.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace Scripts;

/// <summary>
/// Manejador de bases de datos para inicialización y configuración dinámica
/// </summary>
public class DatabaseManager
{
    private readonly ILogger<DatabaseManager> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<DatabaseManager>>();
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    /// <summary>
    /// Inicializa todas las bases de datos de test
    /// </summary>
    public async Task InitializeTestDatabasesAsync(bool force = false, bool skipMigrations = false)
    {
        Console.WriteLine("🔧 Inicializando bases de datos de test...");

        // Advertencia de seguridad para --force
        if (force)
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  ADVERTENCIA: El parámetro --force ELIMINARÁ TODOS LOS DATOS existentes");
            Console.WriteLine("   Esto recreará completamente las siguientes bases de datos:");
            Console.WriteLine("   • usersdb_test");
            Console.WriteLine("   • analysisdb_test");
            Console.WriteLine("   • reportsdb_test");
            Console.WriteLine();
            Console.WriteLine("   Solo continúa si estás seguro de que quieres PERDER TODOS LOS DATOS.");
            Console.WriteLine("   Para mantener los datos existentes, usa el script sin --force");
            Console.WriteLine();
            Console.Write("¿Continuar? (escriba 'SI' para confirmar): ");

            var confirmation = Console.ReadLine();
            if (confirmation?.ToUpper() != "SI")
            {
                Console.WriteLine("❌ Operación cancelada por el usuario.");
                return;
            }
            Console.WriteLine();
        }

        var databases = new Dictionary<string, string>
        {
            ["usersdb_test"] = GetConnectionString("Users"),
            ["analysisdb_test"] = GetConnectionString("Analysis"),
            ["reportsdb_test"] = GetConnectionString("Reports")
        };

        // Procesar cada base de datos
        foreach (var db in databases)
        {
            await ProcessDatabaseAsync(db.Key, db.Value, force, skipMigrations);
        }

        Console.WriteLine("✅ Bases de datos de test inicializadas correctamente");
        PrintUsageOptions();
    }

    /// <summary>
    /// Procesa una base de datos individual
    /// </summary>
    private async Task ProcessDatabaseAsync(string dbName, string connectionString, bool force, bool skipMigrations)
    {
        Console.WriteLine($"📊 Procesando {dbName}...");

        try
        {
            // Verificar si la base de datos existe
            var exists = await DatabaseExistsAsync(connectionString);

            if (exists && !force)
            {
                Console.WriteLine($"   ✅ Base de datos {dbName} ya existe");

                if (!skipMigrations)
                {
                    await ApplyMigrationsAsync(dbName, connectionString);
                }
                return;
            }

            // Crear o recrear base de datos
            if (force && exists)
            {
                Console.WriteLine($"   🔄 Recreando base de datos {dbName}...");
                await DropDatabaseAsync(connectionString);
            }
            else
            {
                Console.WriteLine($"   🆕 Creando base de datos {dbName}...");
            }

            await CreateDatabaseAsync(connectionString);

            if (!skipMigrations)
            {
                await ApplyMigrationsAsync(dbName, connectionString);
            }

            Console.WriteLine($"   ✅ {dbName} configurada correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error procesando {dbName}: {ex.Message}");
            _logger.LogError(ex, "Error procesando base de datos {DatabaseName}", dbName);
        }
    }

    /// <summary>
    /// Verifica si una base de datos existe
    /// </summary>
    private async Task<bool> DatabaseExistsAsync(string connectionString)
    {
        try
        {
            var builder = new DbContextOptionsBuilder<AnalysisDbContext>();
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            using var context = new AnalysisDbContext(builder.Options);
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Crea una base de datos
    /// </summary>
    private async Task CreateDatabaseAsync(string connectionString)
    {
        try
        {
            var builder = new DbContextOptionsBuilder<AnalysisDbContext>();
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            using var context = new AnalysisDbContext(builder.Options);
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando base de datos");
            throw;
        }
    }

    /// <summary>
    /// Elimina una base de datos
    /// </summary>
    private async Task DropDatabaseAsync(string connectionString)
    {
        try
        {
            var builder = new DbContextOptionsBuilder<AnalysisDbContext>();
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            using var context = new AnalysisDbContext(builder.Options);
            await context.Database.EnsureDeletedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando base de datos");
            throw;
        }
    }

    /// <summary>
    /// Aplica migraciones a una base de datos
    /// </summary>
    private async Task ApplyMigrationsAsync(string dbName, string connectionString)
    {
        Console.WriteLine($"   🔄 Aplicando migraciones a {dbName}...");

        try
        {
            var builder = new DbContextOptionsBuilder<AnalysisDbContext>();
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            using var context = new AnalysisDbContext(builder.Options);
            await context.Database.MigrateAsync();

            Console.WriteLine($"   ✅ Migraciones aplicadas a {dbName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error aplicando migraciones a {dbName}: {ex.Message}");
            _logger.LogError(ex, "Error aplicando migraciones a {DatabaseName}", dbName);
        }
    }

    /// <summary>
    /// Obtiene la cadena de conexión para un tipo de base de datos
    /// </summary>
    private string GetConnectionString(string dbType)
    {
        var connectionString = _configuration.GetConnectionString("Default") ??
                             "server=localhost;port=3306;user=root;password=Y0urs3cretOrA7&;TreatTinyAsBoolean=false";

        // Reemplazar el nombre de la base de datos por la versión de test
        var dbName = dbType.ToLower() + "db_test";

        // Usar regex para reemplazar database= con el nombre correcto
        connectionString = Regex.Replace(connectionString,
            @"database=\w+",
            $"database={dbName}",
            RegexOptions.IgnoreCase);

        // Si no había database en la conexión original, agregarlo
        if (!connectionString.Contains("database="))
        {
            connectionString += $";database={dbName}";
        }

        return connectionString;
    }

    /// <summary>
    /// Imprime opciones de uso
    /// </summary>
    private static void PrintUsageOptions()
    {
        Console.WriteLine();
        Console.WriteLine("🚀 Opciones de uso:");
        Console.WriteLine("   dotnet run --project scripts                     # Preservar datos + migraciones");
        Console.WriteLine("   dotnet run --project scripts -- --force          # ⚠️  ELIMINA DATOS + recrea todo");
        Console.WriteLine("   dotnet run --project scripts -- --skip-migrations # Solo crear estructuras");
        Console.WriteLine("   dotnet run --project scripts -- --force --skip-migrations # Recrear sin migraciones");
        Console.WriteLine();
        Console.WriteLine("💡 Consejos:");
        Console.WriteLine("   • Para development normal: sin parámetros");
        Console.WriteLine("   • Para reset completo: --force (elimina datos)");
        Console.WriteLine("   • Para solo estructura: --skip-migrations");
        Console.WriteLine("   • Para tests: dotnet test Analysis.sln --verbosity normal");
    }

    /// <summary>
    /// Limpia datos de las tablas sin eliminar la estructura
    /// </summary>
    public async Task CleanTestDataAsync()
    {
        Console.WriteLine("🧹 Limpiando datos de test (manteniendo estructura)...");

        var databases = new Dictionary<string, string>
        {
            ["usersdb_test"] = GetConnectionString("Users"),
            ["analysisdb_test"] = GetConnectionString("Analysis"),
            ["reportsdb_test"] = GetConnectionString("Reports")
        };

        foreach (var db in databases)
        {
            await CleanDatabaseDataAsync(db.Key, db.Value);
        }

        Console.WriteLine("✅ Datos de test limpiados correctamente");
    }

    /// <summary>
    /// Limpia los datos de una base de datos específica
    /// </summary>
    private async Task CleanDatabaseDataAsync(string dbName, string connectionString)
    {
        Console.WriteLine($"   🧹 Limpiando {dbName}...");

        try
        {
            var builder = new DbContextOptionsBuilder<AnalysisDbContext>();
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            using var context = new AnalysisDbContext(builder.Options);

            // Verificar si la base de datos existe
            if (!await context.Database.CanConnectAsync())
            {
                Console.WriteLine($"   ⚠️  {dbName} no existe, saltando...");
                return;
            }

            // Limpiar tablas en orden (evitar FK constraints)
            await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 0");

            // Obtener todas las tablas
            var tables = await context.Database.SqlQueryRaw<string>(
                "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE()"
            ).ToListAsync();

            foreach (var table in tables)
            {
                if (table != "__EFMigrationsHistory") // Preservar historial de migraciones
                {
                    await context.Database.ExecuteSqlAsync($"TRUNCATE TABLE {table}");
                }
            }

            await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 1");

            Console.WriteLine($"   ✅ {dbName} limpiada correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error limpiando {dbName}: {ex.Message}");
            _logger.LogError(ex, "Error limpiando datos de {DatabaseName}", dbName);
        }
    }
}
