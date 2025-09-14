using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Analysis.Api.Controllers;
using Analysis.Application.Services.Error;
using Analysis.Application.Dtos;

namespace Analysis.Tests.Controllers;

public class ErrorControllerTests
{
    private readonly Mock<IErrorService> _mockService;
    private readonly ErrorController _controller;

    public ErrorControllerTests()
    {
        _mockService = new Mock<IErrorService>();
        _controller = new ErrorController(_mockService.Object);

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

        _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(errors);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Array.Empty<ErrorDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
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

        _mockService.Setup(s => s.GetByResultIdAsync(resultId))
            .ReturnsAsync(errors);

        // Act
        var result = await _controller.GetByResultId(resultId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByResultId_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var resultId = 1;
        _mockService.Setup(s => s.GetByResultIdAsync(resultId))
            .ReturnsAsync(Array.Empty<ErrorDto>());

        // Act
        var result = await _controller.GetByResultId(resultId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated()
    {
        // Arrange
        var createDto = CreateSampleCreateDto();
        var errorDto = CreateSampleErrorDto(1, 1);

        _mockService.Setup(s => s.CreateAsync(It.IsAny<ErrorCreateDto>()))
            .ReturnsAsync(errorDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAll_ShouldReturnOk()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAllAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.DeleteAllAsync(), Times.Once);
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