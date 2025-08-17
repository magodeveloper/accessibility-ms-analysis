using Analysis.Infrastructure;
using FluentValidation;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
// Application services
builder.Services.AddScoped<Analysis.Application.Services.IAnalysisService, Analysis.Application.Services.AnalysisService>();
builder.Services.AddScoped<Analysis.Application.Services.IResultService, Analysis.Application.Services.ResultService>();
builder.Services.AddScoped<Analysis.Application.Services.IErrorService, Analysis.Application.Services.ErrorService>();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Analysis.Application.AnalysisDtoValidator>();

// Controllers
builder.Services.AddControllers()
    .AddDataAnnotationsLocalization()
    .AddViewLocalization();

// Swagger/OpenAPI
builder.Services.AddOpenApi(); // .NET 9
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migración automática de la base de datos al iniciar la API (igual que users)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Analysis.Infrastructure.AnalysisDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

var supportedCultures = new[] { "es", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Middleware global para manejo de errores uniformes con internacionalización
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        // Detectar idioma desde el header Accept-Language o default 'es'
        var lang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        string Get(string key, string lang) => Analysis.Api.Localization.Get(key, lang);
        var result = System.Text.Json.JsonSerializer.Serialize(new { error = Get("Error_InternalServer", lang) });
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(result);
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();