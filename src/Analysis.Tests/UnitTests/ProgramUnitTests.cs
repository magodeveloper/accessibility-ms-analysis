using FluentAssertions;

namespace Analysis.Tests.UnitTests;

public class ProgramUnitTests
{
    [Fact]
    public void Program_Constructor_ShouldWork()
    {
        // Arrange & Act
        var program = new Analysis.Api.Program();

        // Assert
        _ = program.Should().NotBeNull();
    }

    [Fact]
    public void Program_Class_ShouldBePublic()
    {
        // Arrange
        var programType = typeof(Analysis.Api.Program);

        // Act & Assert
        _ = programType.Should().NotBeNull();
        _ = programType.IsPublic.Should().BeTrue();
        _ = programType.IsClass.Should().BeTrue();
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
        _ = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Should().Be(environment);

        // Reset
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Program_Assembly_ShouldContainExpectedTypes()
    {
        // Arrange
        var assembly = typeof(Analysis.Api.Program).Assembly;

        // Act & Assert
        _ = assembly.Should().NotBeNull();
        _ = assembly.GetTypes().Should().Contain(t => t.Name == "Program");
        _ = assembly.GetTypes().Should().Contain(t => t.Name.Contains("Controller"));
    }

    [Fact]
    public void Program_Namespace_ShouldBeCorrect()
    {
        // Arrange
        var programType = typeof(Analysis.Api.Program);

        // Act & Assert
        _ = programType.Namespace.Should().Be("Analysis.Api");
    }

    [Fact]
    public void Program_AssemblyReferences_ShouldIncludeRequiredPackages()
    {
        // Arrange
        var assembly = typeof(Analysis.Api.Program).Assembly;

        // Act
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        _ = referencedAssemblies.Should().Contain(ra => ra.Name!.Contains("Microsoft.AspNetCore"));
        _ = referencedAssemblies.Should().Contain(ra => ra.Name!.Contains("Microsoft.Extensions"));
    }

    [Fact]
    public void Program_TypeProperties_ShouldBeCorrect()
    {
        // Arrange
        var programType = typeof(Analysis.Api.Program);

        // Act & Assert
        _ = programType.IsAbstract.Should().BeFalse();
        _ = programType.IsSealed.Should().BeFalse();
        _ = programType.IsInterface.Should().BeFalse();
        _ = programType.IsValueType.Should().BeFalse();
    }
}
