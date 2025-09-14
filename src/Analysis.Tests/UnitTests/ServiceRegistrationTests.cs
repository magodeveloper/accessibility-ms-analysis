using Analysis.Domain.Services;
using Analysis.Infrastructure;
using Analysis.Infrastructure.Data;
using Analysis.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

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
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<AnalysisDbContext>();
        dbContext.Should().NotBeNull();

        // Verificar que se usa InMemory database
        dbContext.Database.IsInMemory().Should().BeTrue();
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
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<AnalysisDbContext>();
        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUserValidationServiceInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            })
            .Build();

        // Mock configuration para UserValidationService
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUserValidationService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(UserValidationService));
        serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldConfigureHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var httpClientDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IHttpClientFactory));
        httpClientDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            })
            .Build();

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();
        dbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Theory]
    [InlineData("Test")]
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
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<AnalysisDbContext>();
        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
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
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AnalysisDbContext));
        dbContextDescriptor.Should().NotBeNull();

        var userServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUserValidationService));
        userServiceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        // Verificar que todos los servicios requeridos están registrados
        services.Should().Contain(s => s.ServiceType == typeof(AnalysisDbContext));
        services.Should().Contain(s => s.ServiceType == typeof(IUserValidationService));
        services.Should().Contain(s => s.ServiceType == typeof(IHttpClientFactory));
    }
}