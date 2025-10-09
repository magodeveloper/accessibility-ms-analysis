using Moq;
using FluentAssertions;
using Analysis.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.UserContext;

namespace Analysis.Tests.Controllers;

public class ErrorControllerTests
{
    private readonly Mock<IErrorService> _mockService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly ErrorController _controller;

    public ErrorControllerTests()
    {
        _mockService = new Mock<IErrorService>();
        _mockUserContext = new Mock<IUserContext>();

        // Configurar usuario autenticado por defecto
        _ = _mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = _mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _ = _mockUserContext.Setup(x => x.Role).Returns("user");
        _ = _mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = _mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        _controller = new ErrorController(_mockService.Object, _mockUserContext.Object);

        // Setup HttpContext for language helper
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US";
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithErrors()
    {
        // Arrange
        var errors = new[]
        {
            CreateSampleErrorDto(1, 1),
            CreateSampleErrorDto(2, 2)
        };

        _ = _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(errors);

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
            .ReturnsAsync(Array.Empty<ErrorDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task GetByResultId_ShouldReturnOkWithErrors()
    {
        // Arrange
        var resultId = 1;
        var errors = new[]
        {
            CreateSampleErrorDto(1, resultId),
            CreateSampleErrorDto(2, resultId)
        };

        _ = _mockService.Setup(s => s.GetByResultIdAsync(resultId))
            .ReturnsAsync(errors);

        // Act
        var result = await _controller.GetByResultId(resultId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByResultId_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var resultId = 1;
        _ = _mockService.Setup(s => s.GetByResultIdAsync(resultId))
            .ReturnsAsync(Array.Empty<ErrorDto>());

        // Act
        var result = await _controller.GetByResultId(resultId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated()
    {
        // Arrange
        var createDto = CreateSampleCreateDto();
        var errorDto = CreateSampleErrorDto(1, 1);

        _ = _mockService.Setup(s => s.CreateAsync(It.IsAny<ErrorCreateDto>()))
            .ReturnsAsync(errorDto);

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
        var errorId = 1;
        var errorDto = CreateSampleErrorDto(errorId, 1);

        _ = _mockService.Setup(s => s.GetByIdAsync(errorId))
            .ReturnsAsync(errorDto);

        // Act
        var result = await _controller.GetById(errorId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var errorId = 999;
        _ = _mockService.Setup(s => s.GetByIdAsync(errorId))
            .ReturnsAsync((ErrorDto?)null);

        // Act
        var result = await _controller.GetById(errorId);

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
        var errorId = 1;
        _ = _mockService.Setup(s => s.DeleteAsync(errorId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(errorId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.DeleteAsync(errorId), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenErrorNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var errorId = 999;
        _ = _mockService.Setup(s => s.DeleteAsync(errorId))
            .ThrowsAsync(new InvalidOperationException("Error not found"));

        // Act
        var result = await _controller.Delete(errorId);

        // Assert
        _ = result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static ErrorDto CreateSampleErrorDto(int id, int resultId)
    {
        return new ErrorDto(
            Id: id,
            ResultId: resultId,
            WcagCriterionId: 1,
            ErrorCode: "E001",
            Description: "Color contrast insufficient",
            Location: "div.content > p",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );
    }

    private static ErrorCreateDto CreateSampleCreateDto()
    {
        return new ErrorCreateDto(
            ResultId: 1,
            WcagCriterionId: 1,
            ErrorCode: "E001",
            Description: "Color contrast insufficient",
            Location: "div.content > p"
        );
    }
}
