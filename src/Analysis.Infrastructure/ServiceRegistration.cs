using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analysis.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Default")
                 ?? "server=127.0.0.1;port=3306;database=analysisdb;user=msuser;password=msapass;TreatTinyAsBoolean=false";

        services.AddDbContext<AnalysisDbContext>(opt =>
                {
                    opt.UseMySql(
                        cs,
                        ServerVersion.AutoDetect(cs),
                        o => o.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: System.TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null
                        )
                    );
                });

        return services;
    }
}