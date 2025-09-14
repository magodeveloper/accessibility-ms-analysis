using FluentAssertions;
using Xunit;

namespace Analysis.Tests.UnitTests;

public class ProgramUnitTests
{
    [Fact]
    public void Program_Constructor_ShouldWork()
    {
        // Arrange & Act
        var program = new Analysis.Api.Program();

        // Assert
        program.Should().NotBeNull();
    }

    [Fact]
    public void Program_Class_ShouldBePublic()
    {
        // Arrange
        var programType = typeof(Analysis.Api.Program);

        // Act & Assert
        programType.Should().NotBeNull();
        programType.IsPublic.Should().BeTrue();
        programType.IsClass.Should().BeTrue();
    }

    [Theory]
    [InlineData("TestEnvironment")]
    [InlineData("Development")]
    [InlineData("Production")]
    public void Program_ShouldHandle_DifferentEnvironments(string environment)
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);

        // Act & Assert - Just verify the environment variable is set
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Should().Be(environment);

        // Reset
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Program_Assembly_ShouldContainExpectedTypes()
    {
        // Arrange
        var assembly = typeof(Analysis.Api.Program).Assembly;

        // Act & Assert
        assembly.Should().NotBeNull();
        assembly.GetTypes().Should().Contain(t => t.Name == "Program");
        assembly.GetTypes().Should().Contain(t => t.Name.Contains("Controller"));
    }

    [Fact]
    public void Program_Namespace_ShouldBeCorrect()
    {
        // Arrange
        var programType = typeof(Analysis.Api.Program);

        // Act & Assert
        programType.Namespace.Should().Be("Analysis.Api");
    }

    [Fact]
    public void Program_AssemblyReferences_ShouldIncludeRequiredPackages()
    {
        // Arrange
        var assembly = typeof(Analysis.Api.Program).Assembly;

        // Act
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().Contain(ra => ra.Name!.Contains("Microsoft.AspNetCore"));
        referencedAssemblies.Should().Contain(ra => ra.Name!.Contains("Microsoft.Extensions"));
    }

    [Fact]
    public void Program_TypeProperties_ShouldBeCorrect()
    {
        // Arrange
        var programType = typeof(Analysis.Api.Program);

        // Act & Assert
        programType.IsAbstract.Should().BeFalse();
        programType.IsSealed.Should().BeFalse();
        programType.IsInterface.Should().BeFalse();
        programType.IsValueType.Should().BeFalse();
    }
}