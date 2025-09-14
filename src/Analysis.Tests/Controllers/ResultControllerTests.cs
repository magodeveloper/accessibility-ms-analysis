using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Analysis.Api.Controllers;
using Analysis.Application.Services.Result;
using Analysis.Application.Dtos;

namespace Analysis.Tests.Controllers;

public class ResultControllerTests
{
    private readonly Mock<IResultService> _mockService;
    private readonly ResultController _controller;

    public ResultControllerTests()
    {
        _mockService = new Mock<IResultService>();
        _controller = new ResultController(_mockService.Object);

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

        _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(results);

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
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
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

        _mockService.Setup(s => s.GetByAnalysisIdAsync(analysisId))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByAnalysisId_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var analysisId = 1;
        _mockService.Setup(s => s.GetByAnalysisIdAsync(analysisId))
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
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

        _mockService.Setup(s => s.GetByLevelAsync(level))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetByLevel(level);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByLevel_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var level = "violation";
        _mockService.Setup(s => s.GetByLevelAsync(level))
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetByLevel(level);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
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

        _mockService.Setup(s => s.GetBySeverityAsync(severity))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.GetBySeverity(severity);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBySeverity_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var severity = "high";
        _mockService.Setup(s => s.GetBySeverityAsync(severity))
            .ReturnsAsync(Array.Empty<ResultDto>());

        // Act
        var result = await _controller.GetBySeverity(severity);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Create_ShouldReturnCreated()
    {
        // Arrange
        var createDto = CreateSampleCreateDto();
        var resultDto = CreateSampleResultDto(1, 1);

        _mockService.Setup(s => s.CreateAsync(It.IsAny<ResultCreateDto>()))
            .ReturnsAsync(resultDto);

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