using Moq;
using System.Text;
using System.Security.Claims;
using Analysis.Domain.Services;
using Analysis.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analysis.Tests.Infrastructure;

/// <summary>
/// Factory para crear un servidor de pruebas con configuración específica para integration tests.
/// Reemplaza la base de datos MySQL con InMemory y configura servicios de test.
/// </summary>
public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Establecer el entorno de prueba
        builder.UseEnvironment("TestEnvironment");

        // Configurar las opciones de JWT directamente en el builder ANTES de que se construya
        builder.UseSetting("JwtSettings:SecretKey", "KvAuy4?q6DwCSl9Mn+7patFUeX-I^&x5@8%G1d!zkW0iQb2oEhTsP#RYfZNOJ=rc");
        builder.UseSetting("JwtSettings:Issuer", "https://api.accessibility.company.com/analysis");
        builder.UseSetting("JwtSettings:Audience", "https://accessibility.company.com");
        builder.UseSetting("JwtSettings:ExpiryHours", "24");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Limpiar configuraciones existentes y agregar configuración específica de tests
            config.Sources.Clear();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment",
                ["Environment"] = "TestEnvironment",
                ["Gateway:Secret"] = "test-gateway-secret-key",
                ["HealthChecks:MemoryThresholdMB"] = "512",
                ["Metrics:Enabled"] = "true",
                ["ExternalServices:UsersApi:BaseUrl"] = "http://localhost:8081/api",
                ["ExternalServices:ReportsApi:BaseUrl"] = "http://localhost:8083/api",
                // Configuración JWT completa
                ["JwtSettings:SecretKey"] = "KvAuy4?q6DwCSl9Mn+7patFUeX-I^&x5@8%G1d!zkW0iQb2oEhTsP#RYfZNOJ=rc",
                ["JwtSettings:Issuer"] = "https://api.accessibility.company.com/analysis",
                ["JwtSettings:Audience"] = "https://accessibility.company.com",
                ["JwtSettings:ExpiryHours"] = "24"
            });

            // Agregar configuración de test específica
            config.AddJsonFile("appsettings.Test.json", optional: true);
        });

        builder.ConfigureServices(services =>
        {
            // Buscar y remover TODOS los DbContext relacionados para asegurar uso de InMemory
            var descriptors = services.Where(d => d.ServiceType.Name.Contains("DbContext") ||
                                                 d.ServiceType == typeof(DbContextOptions<AnalysisDbContext>) ||
                                                 d.ServiceType == typeof(AnalysisDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
            // Configurar explícitamente el DbContext para usar InMemory
            services.AddDbContext<AnalysisDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            // Reemplazar UserValidationService con un mock que siempre retorna true
            var userValidationDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserValidationService));
            if (userValidationDescriptor != null)
            {
                services.Remove(userValidationDescriptor);
            }

            var mockUserValidationService = new Mock<IUserValidationService>();
            mockUserValidationService.Setup(x => x.ValidateUserExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            services.AddSingleton(mockUserValidationService.Object);

            // Crear la base de datos en memoria
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
            context.Database.EnsureCreated();
        });
    }

    /// <summary>
    /// Genera un token JWT válido para pruebas.
    /// </summary>
    private static string GenerateJwtToken(int userId, string email, string role, string userName)
    {
        var secretKey = "KvAuy4?q6DwCSl9Mn+7patFUeX-I^&x5@8%G1d!zkW0iQb2oEhTsP#RYfZNOJ=rc";
        var issuer = "https://api.accessibility.company.com/analysis";
        var audience = "https://accessibility.company.com";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Crea un HttpClient con headers de autenticación X-User-* y X-Gateway-Secret
    /// para simular peticiones autenticadas por el Gateway.
    /// </summary>
    /// <param name="userId">ID del usuario autenticado (default: 1)</param>
    /// <param name="email">Email del usuario autenticado (default: test@example.com)</param>
    /// <param name="role">Rol del usuario autenticado (default: admin)</param>
    /// <param name="userName">Nombre del usuario autenticado (default: Test User)</param>
    /// <param name="gatewaySecret">Secret del gateway (default: test-gateway-secret-key)</param>
    /// <returns>HttpClient configurado con headers de autenticación</returns>
    public HttpClient CreateAuthenticatedClient(
        int userId = 1,
        string email = "test@example.com",
        string role = "admin",
        string userName = "Test User",
        string gatewaySecret = "test-gateway-secret-key")
    {
        var client = CreateClient();
        var token = GenerateJwtToken(userId, email, role, userName);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", gatewaySecret);
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        client.DefaultRequestHeaders.Add("X-User-Email", email);
        client.DefaultRequestHeaders.Add("X-User-Role", role);
        client.DefaultRequestHeaders.Add("X-User-Name", userName);
        return client;
    }

    /// <summary>
    /// Crea un HttpClient sin headers de autenticación (para tests de middleware).
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Crea un HttpClient solo con gateway secret pero sin user context.
    /// </summary>
    public HttpClient CreateClientWithGatewaySecretOnly(string gatewaySecret = "test-gateway-secret-key")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", gatewaySecret);
        return client;
    }
}
