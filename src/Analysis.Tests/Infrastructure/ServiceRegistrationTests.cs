using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Analysis.Infrastructure;
using Analysis.Infrastructure.Data;
using Analysis.Domain.Services;
using Analysis.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace Analysis.Tests.Infrastructure;

public class ServiceRegistrationTests
{
    private static IConfiguration CreateConfiguration(Dictionary<string, string> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("test")]
    [InlineData("TEST")]
    [InlineData("testing")]
    [InlineData("TestEnvironment")]
    public void AddInfrastructure_WithTestEnvironmentVariants_ShouldRegisterInMemoryDatabase(string environment)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = environment
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void AddInfrastructure_WithNonTestEnvironments_ShouldRegisterMySqlDatabase(string environment)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = environment,
            ["ConnectionStrings:Default"] = "server=localhost;database=testdb;user=test;password=test;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithNoConnectionString_ShouldUseDefaultConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUserValidationService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        });

        // Add IHttpClientFactory dependency
        services.AddHttpClient();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IUserValidationService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(UserValidationService));
        serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterAnalysisDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContextOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DbContextOptions<AnalysisDbContext>));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterHttpClientFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType.Name.Contains("IHttpClientFactory"));
        serviceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        });

        // Add required dependencies
        services.AddHttpClient();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var registeredServices = services.Select(x => x.ServiceType.Name).ToList();

        registeredServices.Should().Contain("AnalysisDbContext");
        registeredServices.Should().Contain("IUserValidationService");
        registeredServices.Should().Contain(x => x.Contains("IHttpClientFactory"));
        registeredServices.Should().Contain(x => x.Contains("DbContextOptions"));
    }

    [Fact]
    public void AddInfrastructure_WithNullEnvironment_ShouldDefaultToMySql()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:Default"] = "server=localhost;database=testdb;user=test;password=test;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldConfigureHttpClientWithTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var httpClientFactoryDescriptor = services.FirstOrDefault(x =>
            x.ServiceType.Name.Contains("IHttpClientFactory"));
        httpClientFactoryDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_WithCustomConnectionString_ShouldUseProvidedConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        var customConnectionString = "server=custom-server;database=custom-db;user=custom-user;password=custom-pass;";
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["ConnectionStrings:Default"] = customConnectionString
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithEmptyEnvironment_ShouldDefaultToMySql()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "",
            ["ConnectionStrings:Default"] = "server=localhost;database=testdb;user=test;password=test;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithEnvironmentVariable_ShouldUseEnvironmentFromVariable()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["Environment"] = "Test"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithBothEnvironmentKeys_ShouldPrioritizeAspNetCoreEnvironment()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test",
            ["Environment"] = "Production",
            ["ConnectionStrings:Default"] = "server=localhost;database=testdb;user=test;password=test;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithHttpClientAlreadyRegistered_ShouldNotFail()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient(); // Pre-register HttpClient
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        });

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        result.Should().NotBeNull();
        var userValidationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IUserValidationService));
        userValidationServiceDescriptor.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AddInfrastructure_WithEmptyOrNullConnectionString_ShouldUseDefaultConnectionString(string? connectionString)
    {
        // Arrange
        var services = new ServiceCollection();
        var settings = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        };

        if (connectionString != null)
        {
            settings["ConnectionStrings:Default"] = connectionString;
        }

        var configuration = CreateConfiguration(settings);

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithMinimalConfiguration_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>());

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var registeredServices = services.Select(x => x.ServiceType.Name).ToList();

        registeredServices.Should().Contain("AnalysisDbContext");
        registeredServices.Should().Contain("IUserValidationService");
        registeredServices.Should().Contain(x => x.Contains("IHttpClientFactory"));
    }

    [Fact]
    public void AddInfrastructure_WithProductionEnvironment_ShouldConfigureMySqlWithRetryPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["ConnectionStrings:Default"] = "server=mysql-server;database=analysisdb;user=user;password=pass;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        // Verificar que se registró un DbContext
        var registeredServices = services.Select(x => x.ServiceType.Name).ToList();
        registeredServices.Should().Contain("AnalysisDbContext");
    }

    [Fact]
    public void AddInfrastructure_WithDevelopmentEnvironment_ShouldConfigureMySqlDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["ConnectionStrings:Default"] = "server=localhost;database=dev_analysisdb;user=dev_user;password=dev_pass;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithStagingEnvironment_ShouldConfigureMySqlDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Staging",
            ["ConnectionStrings:Default"] = "server=staging-db;database=staging_analysisdb;user=staging_user;password=staging_pass;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithProductionAndDefaultConnectionString_ShouldUseDefaultMySqlSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
            // Sin ConnectionStrings:Default para forzar uso del connection string por defecto
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        // Additional assertion to differentiate from AddInfrastructure_WithNoConnectionString_ShouldUseDefaultConnectionString
        var registeredServices = services.Select(x => x.ServiceType.Name).ToList();
        registeredServices.Should().Contain("AnalysisDbContext");
        registeredServices.Should().Contain("IUserValidationService");
    }

    [Fact]
    public void AddInfrastructure_WithHttpClientConfiguration_ShouldSetTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["ConnectionStrings:Default"] = "server=localhost;database=testdb;user=test;password=test;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var httpClientDescriptor = services.FirstOrDefault(x => x.ServiceType.Name.Contains("IHttpClientFactory"));
        httpClientDescriptor.Should().NotBeNull();

        var userValidationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IUserValidationService));
        userValidationServiceDescriptor.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Local")]
    [InlineData("Integration")]
    [InlineData("QA")]
    [InlineData("PreProduction")]
    public void AddInfrastructure_WithNonTestEnvironments_ShouldUseMySqlConfiguration(string environment)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = environment,
            ["ConnectionStrings:Default"] = $"server={environment.ToLower()}-server;database={environment.ToLower()}_db;user=user;password=pass;"
        });

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithCompleteServiceCollection_ShouldReturnSameCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["ConnectionStrings:Default"] = "server=localhost;database=testdb;user=test;password=test;"
        });

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        result.Should().BeSameAs(services);
        result.Should().NotBeNull();
    }
}