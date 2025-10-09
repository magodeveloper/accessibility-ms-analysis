using Moq;
using FluentAssertions;
using Analysis.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.UserContext;

namespace Analysis.Tests.Controllers;

public class ResultControllerTests
{
    private readonly Mock<IResultService> _mockService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly ResultController _controller;

    public ResultControllerTests()
    {
        _mockService = new Mock<IResultService>();
        _mockUserContext = new Mock<IUserContext>();

        // Configurar usuario autenticado por defecto
        _ = _mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = _mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _ = _mockUserContext.Setup(x => x.Role).Returns("user");
        _ = _mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = _mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        _controller = new ResultController(_mockService.Object, _mockUserContext.Object);

        // Setup HttpContext for language helper
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US";
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithResults()
    {
        // Arrange
        var results = new[]
        {
            CreateSampleResultDto(1, 1),
            CreateSampleResultDto(2, 2)
        };

        _ = _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetAll();

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        _ = _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task GetByAnalysisId_ShouldReturnOkWithResults()
    {
        // Arrange
        var analysisId = 1;
        var results = new[]
        {
            CreateSampleResultDto(1, analysisId),
            CreateSampleResultDto(2, analysisId)
        };

        _ = _mockService.Setup(s => s.GetByAnalysisIdAsync(analysisId))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByAnalysisId_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var analysisId = 1;
        _ = _mockService.Setup(s => s.GetByAnalysisIdAsync(analysisId))
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task GetByLevel_ShouldReturnOkWithResults()
    {
        // Arrange
        var level = "violation";
        var results = new[]
        {
            CreateSampleResultDto(1, 1),
            CreateSampleResultDto(2, 2)
        };

        _ = _mockService.Setup(s => s.GetByLevelAsync(level))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetByLevel(level);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByLevel_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var level = "violation";
        _ = _mockService.Setup(s => s.GetByLevelAsync(level))
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetByLevel(level);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task GetBySeverity_ShouldReturnOkWithResults()
    {
        // Arrange
        var severity = "high";
        var results = new[]
        {
            CreateSampleResultDto(1, 1),
            CreateSampleResultDto(2, 2)
        };

        _ = _mockService.Setup(s => s.GetBySeverityAsync(severity))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetBySeverity(severity);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBySeverity_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var severity = "high";
        _ = _mockService.Setup(s => s.GetBySeverityAsync(severity))
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetBySeverity(severity);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Create_ShouldReturnCreated()
    {
        // Arrange
        var createDto = CreateSampleCreateDto();
        var resultDto = CreateSampleResultDto(1, 1);

        _ = _mockService.Setup(s => s.CreateAsync(It.IsAny<ResultCreateDto>()))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        _ = result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        _ = createdResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAll_ShouldReturnOk()
    {
        // Arrange
        _ = _mockService.Setup(s => s.DeleteAllAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAll();

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.DeleteAllAsync(), Times.Once);
    }

    // ==================== Edge Cases - GetById ====================

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var resultId = 1;
        var resultDto = CreateSampleResultDto(resultId, 1);

        _ = _mockService.Setup(s => s.GetByIdAsync(resultId))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.GetById(resultId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var resultId = 999;
        _ = _mockService.Setup(s => s.GetByIdAsync(resultId))
            .ReturnsAsync((ResultDto?)null);

        // Act
        var result = await _controller.GetById(resultId);

        // Assert
        _ = result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ==================== Edge Cases - Authentication ====================

    [Fact]
    public async Task Create_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);
        var createDto = CreateSampleCreateDto();

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        _ = result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        _ = unauthorizedResult!.StatusCode.Should().Be(401);
    }

    // ==================== Edge Cases - Delete ====================

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var resultId = 1;
        _ = _mockService.Setup(s => s.DeleteAsync(resultId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(resultId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.DeleteAsync(resultId), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenResultNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var resultId = 999;
        _ = _mockService.Setup(s => s.DeleteAsync(resultId))
            .ThrowsAsync(new InvalidOperationException("Result not found"));

        // Act
        var result = await _controller.Delete(resultId);

        // Assert
        _ = result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static ResultDto CreateSampleResultDto(int id, int analysisId)
    {
        return new ResultDto(
            Id: id,
            AnalysisId: analysisId,
            WcagCriterionId: 1,
            WcagCriterion: "1.4.3 Contrast (Minimum)",
            Level: "violation",
            Severity: "high",
            Description: "Elements must have sufficient color contrast",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );
    }

    private static ResultCreateDto CreateSampleCreateDto()
    {
        return new ResultCreateDto(
            AnalysisId: 1,
            WcagCriterionId: 1,
            WcagCriterion: "1.4.3 Contrast (Minimum)",
            Level: "violation",
            Severity: "high",
            Description: "Elements must have sufficient color contrast"
        );
    }
}
