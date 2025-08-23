using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Analysis.Infrastructure.Data;
using Scripts;

var builder = Host.CreateApplicationBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configurar configuración
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("../src/Analysis.Api/appsettings.json", optional: true)
    .AddJsonFile("../src/Analysis.Api/appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Registrar servicios
builder.Services.AddDbContext<AnalysisDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<DatabaseManager>();

var host = builder.Build();

// Ejecutar el administrador de bases de datos
using var scope = host.Services.CreateScope();
var dbManager = scope.ServiceProvider.GetRequiredService<DatabaseManager>();

// Parsear argumentos
var force = args.Contains("--force") || args.Contains("-f");
var skipMigrations = args.Contains("--skip-migrations") || args.Contains("-s");
var cleanData = args.Contains("--clean-data") || args.Contains("-c");

try
{
    if (cleanData)
    {
        await dbManager.CleanTestDataAsync();
    }
    else
    {
        await dbManager.InitializeTestDatabasesAsync(force, skipMigrations);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error inicializando bases de datos: {ex.Message}");
    Environment.Exit(1);
}
