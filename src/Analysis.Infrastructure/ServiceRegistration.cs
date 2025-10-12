using Analysis.Domain.Services;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Analysis.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analysis.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Detectar si estamos en entorno de tests
        var environmentName = config["ASPNETCORE_ENVIRONMENT"] ?? config["Environment"];

        if (environmentName == "TestEnvironment")
        {
            // Para tests, usar InMemory database
            services.AddDbContext<AnalysisDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));
        }
        else
        {
            // Para producción/desarrollo, usar MySQL
            var cs = config.GetConnectionString("Default")
                     ?? "server=127.0.0.1;port=3306;database=analysisdb;user=msuser;password=msapass;TreatTinyAsBoolean=false";

            services.AddDbContext<AnalysisDbContext>(opt =>
            {
                opt.UseMySql(
                    cs,
                    new MySqlServerVersion(new Version(8, 4, 6)),
                    o => o.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: System.TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                );
            });
        }

        // Registrar servicios de dominio
        services.AddScoped<IUserValidationService, UserValidationService>();

        // Configurar HttpClient para comunicación con otros microservicios
        services.AddHttpClient<UserValidationService>(client =>
        {
            // Configuración por defecto - puede ser sobrescrita por configuración
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
