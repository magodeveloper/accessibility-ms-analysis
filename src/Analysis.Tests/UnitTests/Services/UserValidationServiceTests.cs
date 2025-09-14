using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;
using Analysis.Infrastructure.Services;

namespace Analysis.Tests.UnitTests.Services;

public class UserValidationServiceTests
{
    private readonly Mock<ILogger<UserValidationService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public UserValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<UserValidationService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        // Create real configuration with in-memory data
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalServices:UsersApi:BaseUrl"] = "https://api.users.com",
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["Testing:SkipUserValidation"] = "false"
        }!);
        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBaseUrl_ShouldThrowException()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>()!);
        var config = configBuilder.Build();

        // Act & Assert
        Action act = () => new UserValidationService(_httpClient, _mockLogger.Object, config);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Users API base URL is not configured*");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_InTestEnvironment_ShouldReturnTrue()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalServices:UsersApi:BaseUrl"] = "https://api.users.com",
            ["ASPNETCORE_ENVIRONMENT"] = "Test"
        }!);
        var config = configBuilder.Build();

        var service = new UserValidationService(_httpClient, _mockLogger.Object, config);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains("Test environment detected");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_InDevelopmentWithSkipValidation_ShouldReturnTrue()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ExternalServices:UsersApi:BaseUrl"] = "https://api.users.com",
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["Testing:SkipUserValidation"] = "true"
        }!);
        var config = configBuilder.Build();

        var service = new UserValidationService(_httpClient, _mockLogger.Object, config);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains("Test environment detected");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_UserExists_ShouldReturnTrue()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"id\":1,\"name\":\"Test User\"}");
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeTrue();
        VerifyHttpRequest("https://api.users.com/api/users/1");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_UserNotFound_ShouldReturnFalse()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "");
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeFalse();
        VerifyLogContains("User 1 not found");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_ServerError_ShouldReturnFalse()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "");
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeFalse();
        VerifyLogContains("Failed to validate user 1");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_NetworkError_ShouldReturnTrue()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeTrue(); // En caso de error de red, permitir la operación
        VerifyLogContains("Network error validating user 1");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_Timeout_ShouldReturnFalse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeFalse();
        VerifyLogContains("Timeout validating user 1");
    }

    [Fact]
    public async Task ValidateUserExistsAsync_UnexpectedError_ShouldReturnFalse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserExistsAsync(1);

        // Assert
        result.Should().BeFalse();
        VerifyLogContains("Unexpected error validating user 1");
    }

    [Fact]
    public async Task ValidateUserCanAccessAnalysisAsync_ValidUser_ShouldReturnTrue()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"id\":1,\"name\":\"Test User\"}");
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserCanAccessAnalysisAsync(1, 100);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUserCanAccessAnalysisAsync_InvalidUser_ShouldReturnFalse()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "");
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserCanAccessAnalysisAsync(1, 100);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUserCanAccessAnalysisAsync_Exception_ShouldReturnFalse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);

        // Act
        var result = await service.ValidateUserCanAccessAnalysisAsync(1, 100);

        // Assert
        result.Should().BeFalse();
        VerifyLogContains("Unexpected error validating user 1");
    }

    [Fact]
    public async Task ValidateMultipleUsersAsync_MultipleUsers_ShouldReturnDictionary()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"id\":1,\"name\":\"Test User\"}");
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);
        var userIds = new[] { 1, 2, 3 };

        // Act
        var result = await service.ValidateMultipleUsersAsync(userIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Keys.Should().Contain(userIds);
        result.Values.Should().AllSatisfy(v => v.Should().BeTrue());
    }

    [Fact]
    public async Task ValidateMultipleUsersAsync_EmptyList_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);
        var userIds = Array.Empty<int>();

        // Act
        var result = await service.ValidateMultipleUsersAsync(userIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateMultipleUsersAsync_MixedResults_ShouldReturnCorrectDictionary()
    {
        // Arrange
        var responseCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                responseCount++;
                var statusCode = responseCount == 1 ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                return new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
            });

        var service = new UserValidationService(_httpClient, _mockLogger.Object, _configuration);
        var userIds = new[] { 1, 2 };

        // Act
        var result = await service.ValidateMultipleUsersAsync(userIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainKey(1);
        result.Should().ContainKey(2);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void VerifyHttpRequest(string expectedUri)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == expectedUri),
                ItExpr.IsAny<CancellationToken>());
    }

    private void VerifyLogContains(string expectedMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}