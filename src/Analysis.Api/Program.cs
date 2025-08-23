using System.Linq;
using System.Text.Json;
using FluentValidation;
using Analysis.Application;
using Microsoft.OpenApi.Any;
using Analysis.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using FluentValidation.AspNetCore;
using Analysis.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.Analysis;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer(); // Explorador de API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Analysis API",
        Version = "v1"
    });
});

// Controllers
builder.Services.AddControllers() // Controladores
    .AddDataAnnotationsLocalization() // Localización de anotaciones de datos
    .AddViewLocalization(); // Localización de vistas

var app = builder.Build(); // Construcción de la aplicación

// Migración automática de la base de datos al iniciar la API - solo en producción/desarrollo
var environment = app.Environment.EnvironmentName;
if (environment != "Test" && environment != "TestEnvironment")
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
        await db.Database.MigrateAsync();
    }
}
else
{
    // Para tests, solo asegurar que la base de datos se cree
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}

var supportedCultures = new[] { "es", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Middleware global para manejo de errores uniformes
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        // Detectar idioma desde el header Accept-Language o default 'es'
        var lang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        string Get(string key, string lang) => Analysis.Application.Localization.Get(key, lang);
        var result = JsonSerializer.Serialize(new { error = Get("Error_InternalServer", lang) });
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(result);
    });
});

// app.UseHttpsRedirection();

// app.UseAuthorization();

// Habilitar el enrutamiento de controladores
app.MapControllers();

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