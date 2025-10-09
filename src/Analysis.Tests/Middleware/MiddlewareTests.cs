using Moq;
using FluentAssertions;
using System.Security.Claims;
using Analysis.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Analysis.Application.Services.UserContext;

namespace Analysis.Tests.Middleware;

/// <summary>
/// Tests unitarios para los Middlewares del microservicio Analysis
/// </summary>
public class MiddlewareTests
{
    #region GatewaySecretValidationMiddleware Tests

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithValidSecret_CallsNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "test-secret-123";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithMissingSecret_Returns403()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeFalse();
        _ = context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        _ = responseBody.Should().Contain("Forbidden");
        _ = responseBody.Should().Contain("Direct access to microservice is not allowed");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithInvalidSecret_Returns403()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "wrong-secret";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeFalse();
        _ = context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        _ = responseBody.Should().Contain("Invalid Gateway secret");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WhenSecretNotConfigured_SkipsValidation()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns((string?)null);
        _ = configuration.Setup(c => c["GATEWAY_SECRET"]).Returns((string?)null);

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_ReadsSecretFromAlternativeConfig()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns((string?)null);
        _ = configuration.Setup(c => c["GATEWAY_SECRET"]).Returns("env-secret-456");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "env-secret-456";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithMissingSecret_LogsWarning()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, mockLogger.Object, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/analysis";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Missing X-Gateway-Secret header")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithInvalidSecret_LogsWarning()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, mockLogger.Object, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/analysis";
        context.Request.Headers["X-Gateway-Secret"] = "wrong-secret";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid X-Gateway-Secret header")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithValidSecret_LogsDebug()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, mockLogger.Object, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/analysis";
        context.Request.Headers["X-Gateway-Secret"] = "test-secret-123";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Gateway secret validated successfully")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void GatewaySecretValidationMiddleware_WhenSecretNotConfigured_LogsWarningOnConstruction()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns((string?)null);
        _ = configuration.Setup(c => c["GATEWAY_SECRET"]).Returns((string?)null);

        // Act
        _ = new GatewaySecretValidationMiddleware(next, mockLogger.Object, configuration.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Gateway:Secret not configured")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithEmptySecretHeader_Returns403()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeFalse();
        _ = context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        _ = responseBody.Should().Contain("Invalid Gateway secret");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithWhitespaceSecret_Returns403()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "   ";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeFalse();
        _ = context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        _ = responseBody.Should().Contain("Invalid Gateway secret");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithCaseSensitiveSecret_Returns403()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-SECRET-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "test-secret-123"; // lowercase
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeFalse();
        _ = context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        _ = responseBody.Should().Contain("Invalid Gateway secret");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_ResponseContentType_IsApplicationJson()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _ = context.Response.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithConfiguredEmptySecret_SkipsValidation()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("");
        _ = configuration.Setup(c => c["GATEWAY_SECRET"]).Returns("");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_ErrorResponse_ContainsExpectedStructure()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        _ = responseBody.Should().Contain("error");
        _ = responseBody.Should().Contain("message");
        _ = responseBody.Should().Contain("Forbidden");
    }

    #endregion

    #region GatewaySecretValidationMiddlewareExtensions Tests

    [Fact]
    public void UseGatewaySecretValidation_RegistersMiddleware()
    {
        // Arrange
        var appBuilder = new Mock<IApplicationBuilder>();
        _ = appBuilder.Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                  .Returns(appBuilder.Object);

        // Act
        var result = appBuilder.Object.UseGatewaySecretValidation();

        // Assert
        _ = result.Should().NotBeNull();
        appBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    #endregion

    #region UserContextMiddleware Tests

    [Fact]
    public async Task UserContextMiddleware_WithValidHeaders_PopulatesUserContext()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "123";
        context.Request.Headers["X-User-Email"] = "test@example.com";
        context.Request.Headers["X-User-Role"] = "Admin";
        context.Request.Headers["X-User-Name"] = "Test User";

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = userContext.UserId.Should().Be(123);
        _ = userContext.Email.Should().Be("test@example.com");
        _ = userContext.Role.Should().Be("Admin");
        _ = userContext.UserName.Should().Be("Test User");
    }

    [Fact]
    public async Task UserContextMiddleware_WithMissingHeaders_LeavesUserContextEmpty()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = userContext.UserId.Should().Be(0);
        _ = userContext.Email.Should().Be(string.Empty);
        _ = userContext.Role.Should().Be(string.Empty);
        _ = userContext.UserName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task UserContextMiddleware_WithInvalidUserId_DoesNotPopulateContext()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "invalid-id";
        context.Request.Headers["X-User-Email"] = "test@example.com";

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = userContext.UserId.Should().Be(0);
    }

    [Fact]
    public async Task UserContextMiddleware_WithPartialHeaders_PopulatesAvailableFields()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "456";
        context.Request.Headers["X-User-Email"] = "partial@example.com";
        // No se incluyen X-User-Role ni X-User-Name

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = userContext.UserId.Should().Be(456);
        _ = userContext.Email.Should().Be("partial@example.com");
        _ = userContext.Role.Should().Be(string.Empty);
        _ = userContext.UserName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task UserContextMiddleware_WithEmptyHeaders_UsesEmptyStrings()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "789";
        context.Request.Headers["X-User-Email"] = "";
        context.Request.Headers["X-User-Role"] = "";
        context.Request.Headers["X-User-Name"] = "";

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = nextCalled.Should().BeTrue();
        _ = userContext.UserId.Should().Be(789);
        _ = userContext.Email.Should().Be(string.Empty);
        _ = userContext.Role.Should().Be(string.Empty);
        _ = userContext.UserName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task UserContextMiddleware_WhenExceptionOccurs_ContinuesProcessing()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();

        // Crear un mock de IUserContext que no sea UserContext (para causar null en el cast)
        var mockUserContext = new Mock<IUserContext>();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "123";

        // Act
        await middleware.InvokeAsync(context, mockUserContext.Object);

        // Assert
        // El middleware debe continuar incluso si no puede poblar el contexto
        _ = nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task UserContextMiddleware_WithValidHeaders_LogsInformation()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "123";
        context.Request.Headers["X-User-Email"] = "test@example.com";
        context.Request.Headers["X-User-Role"] = "Admin";

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User context populated")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task UserContextMiddleware_WithMissingHeaders_LogsDebug()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No authentication found - anonymous request")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task UserContextMiddleware_WithJwtClaims_PopulatesUserContext()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", "999"),
            new Claim("email", "jwt@example.com"),
            new Claim("name", "JWT User"),
            new Claim("role", "User")
        ], "TestAuth");

        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = userContext.UserId.Should().Be(999);
        _ = userContext.Email.Should().Be("jwt@example.com");
        _ = userContext.UserName.Should().Be("JWT User");
        _ = userContext.Role.Should().Be("User");
    }

    [Fact]
    public async Task UserContextMiddleware_WithJwtClaims_AlternativeClaimTypes_PopulatesUserContext()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
        [
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "888"),
            new Claim("email", "alt@example.com"),
            new Claim("name", "Alt User"),
            new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin")
        ], "TestAuth");

        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = userContext.UserId.Should().Be(888);
        _ = userContext.Email.Should().Be("alt@example.com");
        _ = userContext.UserName.Should().Be("Alt User");
        _ = userContext.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task UserContextMiddleware_WithJwtClaims_MissingUserId_DoesNotPopulateContext()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
        [
            new Claim("email", "noid@example.com"),
            new Claim("name", "No ID User")
        ], "TestAuth");

        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = userContext.UserId.Should().Be(0);
        _ = userContext.Email.Should().Be(string.Empty);
    }

    [Fact]
    public async Task UserContextMiddleware_WithJwtClaims_InvalidUserId_DoesNotPopulateContext()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", "not-a-number"),
            new Claim("email", "invalid@example.com")
        ], "TestAuth");

        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        _ = userContext.UserId.Should().Be(0);
    }

    [Fact]
    public async Task UserContextMiddleware_WithJwtClaims_LogsInformation()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", "777"),
            new Claim("email", "jwt@example.com"),
            new Claim("role", "User")
        ], "TestAuth");

        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User context populated from JWT")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task UserContextMiddleware_HeadersTakePriorityOverJwt()
    {
        // Arrange
        static Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        // Configurar headers (prioridad 1)
        context.Request.Headers["X-User-Id"] = "100";
        context.Request.Headers["X-User-Email"] = "header@example.com";
        context.Request.Headers["X-User-Role"] = "HeaderRole";
        context.Request.Headers["X-User-Name"] = "Header User";

        // Configurar JWT claims (prioridad 2)
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", "200"),
            new Claim("email", "jwt@example.com"),
            new Claim("name", "JWT User"),
            new Claim("role", "JwtRole")
        ], "TestAuth");

        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert - Los headers deben tener prioridad
        _ = userContext.UserId.Should().Be(100);
        _ = userContext.Email.Should().Be("header@example.com");
        _ = userContext.Role.Should().Be("HeaderRole");
        _ = userContext.UserName.Should().Be("Header User");
    }

    [Fact]
    public async Task UserContextMiddleware_WithNonUserContextType_LogsWarningAndContinues()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var mockLogger = new Mock<ILogger<UserContextMiddleware>>();

        // Crear un mock que no sea UserContext (para causar el warning)
        var mockUserContext = new Mock<IUserContext>();

        var middleware = new UserContextMiddleware(next, mockLogger.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "123";

        // Act
        await middleware.InvokeAsync(context, mockUserContext.Object);

        // Assert
        _ = nextCalled.Should().BeTrue();
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("UserContext is not of type UserContext")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void UseUserContext_RegistersMiddleware()
    {
        // Arrange
        var appBuilder = new Mock<IApplicationBuilder>();
        _ = appBuilder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appBuilder.Object);

        // Act
        var result = appBuilder.Object.UseUserContext();

        // Assert
        _ = result.Should().NotBeNull();
        appBuilder.Verify(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    #endregion
}
