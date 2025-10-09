using Prometheus;
using System.Text;
using FluentValidation;
using System.Text.Json;
using Analysis.Api.Middleware;
using Analysis.Infrastructure;
using Microsoft.OpenApi.Models;
using Analysis.Api.HealthChecks;
using FluentValidation.AspNetCore;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.Analysis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Tags para health checks
string[] tags = ["live", "ready"];

builder.Services.AddOpenApi(); // .NET 9

// Registrar FluentValidation y controladores MVC
builder.Services.AddControllers(); // Controladores
builder.Services.AddValidatorsFromAssemblyContaining<Analysis.Application.Dtos.AnalysisCreateDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddInfrastructure(builder.Configuration); // Infraestructura

// Servicios de aplicación y dominio
builder.Services.AddScoped<IAnalysisService, AnalysisService>(); // Servicio de análisis
builder.Services.AddScoped<IResultService, ResultService>(); // Servicio de resultados
builder.Services.AddScoped<IErrorService, ErrorService>(); // Servicio de errores

// User Context Service - Extrae información del usuario de los headers X-User-* del Gateway
builder.Services.AddScoped<Analysis.Application.Services.UserContext.IUserContext, Analysis.Application.Services.UserContext.UserContext>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer(); // Explorador de API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Analysis API",
        Version = "v1"
    });

    // Configuración de autenticación JWT Bearer en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT en el formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Controllers
builder.Services.AddControllers() // Controladores
    .AddDataAnnotationsLocalization() // Localización de anotaciones de datos
    .AddViewLocalization(); // Localización de vistas

// --- JWT Authentication Configuration ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JwtSettings:SecretKey is required");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- Health Checks Configuration ---
var healthChecksConfig = builder.Configuration.GetSection("HealthChecks");
var memoryThresholdMB = healthChecksConfig.GetValue<long>("MemoryThresholdMB", 512);
var memoryThresholdBytes = memoryThresholdMB * 1024L * 1024L;

// Registrar health checks como servicios
builder.Services.AddSingleton<IHealthCheck>(sp =>
    new MemoryHealthCheck(
        sp.GetRequiredService<ILogger<MemoryHealthCheck>>(),
        memoryThresholdBytes));

var healthChecksBuilder = builder.Services.AddHealthChecks()
    // Health check básico de la aplicación
    .AddCheck<ApplicationHealthCheck>(
        "application",
        tags: tags)

    // Health check de memoria  
    .AddCheck<MemoryHealthCheck>(
        "memory",
        tags: new[] { "live" })

    // Health check de base de datos personalizado
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: new[] { "ready" })

    // Health check de EF Core
    .AddDbContextCheck<AnalysisDbContext>(
        "analysis_dbcontext",
        tags: new[] { "ready" });

// Health check de MySQL (opcional, requiere connection string)
var connectionString = builder.Configuration.GetConnectionString("Default");
if (!string.IsNullOrEmpty(connectionString))
{
    healthChecksBuilder.AddMySql(
        connectionString,
        name: "mysql",
        tags: new[] { "ready", "database" });
}

var app = builder.Build(); // Construcción de la aplicación

// Migración automática de la base de datos al iniciar la API
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var environment = env.EnvironmentName;

    // Lista de entornos que usan EnsureCreated
    var testEnvironments = new[] { "TestEnvironment", "Testing", "Test", "UnitTest", "IntegrationTest", "Development" };

    if (testEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase))
    {
        // Para entornos de test/desarrollo, solo crear la base de datos
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        // Para producción con MySQL, ejecutar migraciones
        await db.Database.MigrateAsync();
    }
}

var supportedCultures = new[] { "es", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Gateway Secret Validation - Valida que las peticiones vengan del Gateway
app.UseGatewaySecretValidation();

// Habilitar autenticación y autorización JWT
app.UseAuthentication();
app.UseAuthorization();

// User Context Middleware - Extrae información del usuario de headers X-User-*
app.UseUserContext();

// Middleware global para manejo de errores uniformes
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        // Detectar idioma desde el header Accept-Language o default 'es'
        var lang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";

        static string Get(string key, string lang) => Analysis.Application.Localization.Get(key, lang);
        var result = JsonSerializer.Serialize(new { error = Get("Error_InternalServer", lang) });
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(result);
    });
});

// app.UseHttpsRedirection();

// app.UseAuthorization();

// Habilitar el enrutamiento de controladores
app.MapControllers();

// --- Prometheus Metrics ---
var metricsConfig = builder.Configuration.GetSection("Metrics");
var metricsEnabled = metricsConfig.GetValue<bool>("Enabled", true);

if (metricsEnabled)
{
    app.UseHttpMetrics();
}

// --- Health Check Endpoints ---
// Endpoint de liveness: verifica que la aplicación está viva (no verifica dependencias)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Endpoint de readiness: verifica que la aplicación está lista para recibir tráfico
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Endpoint de health general: devuelve el estado completo
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    tags = e.Value.Tags,
                    data = e.Value.Data,
                    exception = e.Value.Exception?.Message
                })
        }, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        await context.Response.WriteAsync(result);
    }
});

// --- Prometheus Metrics Endpoint ---
if (metricsEnabled)
{
    app.MapMetrics(); // Endpoint /metrics para Prometheus
}

// Swagger/OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();

// Necesario para tests de integración (WebApplicationFactory)
namespace Analysis.Api
{
    public partial class Program { public Program() { } }
}
