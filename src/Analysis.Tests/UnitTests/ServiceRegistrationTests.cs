using FluentAssertions;
using Analysis.Infrastructure;
using Analysis.Domain.Services;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Analysis.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analysis.Tests.UnitTests;

/// <summary>
/// Tests para ServiceRegistration de Infrastructure (solo configuraciones que funcionan sin MySQL)
/// </summary>
public class ServiceRegistrationTests
{
    [Fact]
    public void AddInfrastructure_WithTestEnvironment_ShouldUseInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment"
            })
            .Build();

        // Act
        _ = services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<AnalysisDbContext>();
        _ = dbContext.Should().NotBeNull();

        // Verificar que se usa InMemory database
        _ = dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructure_WithTestEnvironmentVariable_ShouldUseInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Environment"] = "TestEnvironment"
            })
            .Build();

        // Act
        _ = services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<AnalysisDbContext>();
        _ = dbContext.Should().NotBeNull();
        _ = dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUserValidationServiceInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment"
            })
            .Build();

        // Mock configuration para UserValidationService
        _ = services.AddSingleton<IConfiguration>(configuration);
        _ = services.AddLogging();

        // Act
        _ = services.AddInfrastructure(configuration);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUserValidationService));
        _ = serviceDescriptor.Should().NotBeNull();
        _ = serviceDescriptor!.ImplementationType.Should().Be<UserValidationService>();
        _ = serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldConfigureHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment"
            })
            .Build();

        // Act
        _ = services.AddInfrastructure(configuration);

        // Assert
        var httpClientDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IHttpClientFactory));
        _ = httpClientDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment"
            })
            .Build();

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        _ = result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment"
            })
            .Build();

        // Act
        _ = services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AnalysisDbContext));
        _ = dbContextDescriptor.Should().NotBeNull();
        _ = dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Theory]
    [InlineData("TestEnvironment")]
    public void AddInfrastructure_WithTestEnvironments_ShouldUseInMemoryDatabase(string environment)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = environment
            })
            .Build();

        // Act
        _ = services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<AnalysisDbContext>();
        _ = dbContext.Should().NotBeNull();
        _ = dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructure_WithNullEnvironment_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // No environment specified - esto no causará problemas de MySQL si no intentamos acceder a la DB
            })
            .Build();

        // Act
        _ = services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AnalysisDbContext));
        _ = dbContextDescriptor.Should().NotBeNull();

        var userServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUserValidationService));
        _ = userServiceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment"
            })
            .Build();

        // Act
        _ = services.AddInfrastructure(configuration);

        // Assert
        // Verificar que todos los servicios requeridos están registrados
        _ = services.Should().Contain(s => s.ServiceType == typeof(AnalysisDbContext));
        _ = services.Should().Contain(s => s.ServiceType == typeof(IUserValidationService));
        _ = services.Should().Contain(s => s.ServiceType == typeof(IHttpClientFactory));
    }
}
