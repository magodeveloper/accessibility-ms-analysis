using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analysis.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? "server=127.0.0.1;port=3306;database=ms_analysis;user=msa;password=msapass;TreatTinyAsBoolean=false";

        services.AddDbContext<Analysis.Infrastructure.Data.AnalysisDbContext>(opt =>
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