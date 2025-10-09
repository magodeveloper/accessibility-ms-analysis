using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Analysis.Application.Dtos;
using Analysis.Api.Controllers;
using Analysis.Application.Services.Analysis;
using Analysis.Application.Services.UserContext;

namespace Analysis.Tests.Controllers;

public class AnalysisControllerTests
{
    private readonly Mock<IAnalysisService> _mockService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly AnalysisController _controller;

    public AnalysisControllerTests()
    {
        _mockService = new Mock<IAnalysisService>();
        _mockUserContext = new Mock<IUserContext>();

        // Configurar usuario autenticado por defecto
        _ = _mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = _mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _ = _mockUserContext.Setup(x => x.Role).Returns("user");
        _ = _mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = _mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        _controller = new AnalysisController(_mockService.Object, _mockUserContext.Object);

        // Setup HttpContext for language helper
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US";
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithAnalyses()
    {
        // Arrange
        var analyses = new[]
        {
            CreateSampleAnalysisDto(1, 1),
            CreateSampleAnalysisDto(2, 2)
        };

        _ = _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetAll();

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUser_WithValidUserId_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var analyses = new[]
        {
            CreateSampleAnalysisDto(1, userId),
            CreateSampleAnalysisDto(2, userId)
        };

        _ = _mockService.Setup(s => s.GetByUserIdAsync(userId))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetByUser(userId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var analysisId = 1;
        var analysis = CreateSampleAnalysisDto(analysisId, 1);

        _ = _mockService.Setup(s => s.GetByIdAsync(analysisId))
            .ReturnsAsync(analysis);

        // Act
        var result = await _controller.GetById(analysisId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        _ = okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var analysisId = 999;
        _ = _mockService.Setup(s => s.GetByIdAsync(analysisId))
            .ReturnsAsync((AnalysisDto?)null);

        // Act
        var result = await _controller.GetById(analysisId);

        // Assert
        _ = result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new AnalysisCreateDto(
            UserId: 1,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "html",
            ContentInput: "<html></html>",
            SourceUrl: "https://example.com",
            ToolUsed: "axe-core",
            Status: "success",
            SummaryResult: "Analysis completed",
            ResultJson: "{}",
            DurationMs: 100,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 0,
            AxeNeedsReview: 0,
            AxeRecommendations: 0,
            AxePasses: 5,
            AxeIncomplete: 0,
            AxeInapplicable: 0,
            EaViolations: 0,
            EaNeedsReview: 0,
            EaRecommendations: 0,
            EaPasses: 5,
            EaIncomplete: 0,
            EaInapplicable: 0
        );

        var expectedResult = CreateSampleAnalysisDto(1, 1);

        _ = _mockService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        _ = result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        _ = createdResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByDate_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var date = DateTime.Today;
        var analyses = new[]
        {
            CreateSampleAnalysisDto(1, userId),
            CreateSampleAnalysisDto(2, userId)
        };

        _ = _mockService.Setup(s => s.GetByDateAsync(userId, date))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetByDate(userId, date);

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
            .ReturnsAsync(Array.Empty<AnalysisDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByUser_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        _ = _mockService.Setup(s => s.GetByUserIdAsync(userId))
            .ReturnsAsync(Array.Empty<AnalysisDto>());

        // Act
        var result = await _controller.GetByUser(userId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByDate_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var date = DateTime.Today;
        _ = _mockService.Setup(s => s.GetByDateAsync(userId, date))
            .ReturnsAsync(Array.Empty<AnalysisDto>());

        // Act
        var result = await _controller.GetByDate(userId, date);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
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

    // ==================== Edge Cases - Authentication & Authorization ====================

    [Fact]
    public async Task GetAll_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _controller.GetAll();

        // Assert
        _ = result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        _ = unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetByUser_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _controller.GetByUser(1);

        // Assert
        _ = result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        _ = unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetByUser_WhenUserAccessingOtherUserDataAndNotAdmin_ShouldReturnForbid()
    {
        // Arrange
        _ = _mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = _mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        // Act - User 1 trying to access User 2's data
        var result = await _controller.GetByUser(2);

        // Assert
        _ = result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetByUser_WhenAdminAccessingOtherUserData_ShouldReturnOk()
    {
        // Arrange
        _ = _mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = _mockUserContext.Setup(x => x.IsAdmin).Returns(true);

        var analyses = new[] { CreateSampleAnalysisDto(1, 2) };
        _ = _mockService.Setup(s => s.GetByUserIdAsync(2))
            .ReturnsAsync(analyses);

        // Act - Admin (User 1) accessing User 2's data
        var result = await _controller.GetByUser(2);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }

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

    [Fact]
    public async Task Create_ShouldOverrideUserIdWithAuthenticatedUserId()
    {
        // Arrange
        _ = _mockUserContext.Setup(x => x.UserId).Returns(5);
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);

        // CreateDto with UserId=1, but should be overridden to 5
        var createDto = new AnalysisCreateDto(
            UserId: 1,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "html",
            ContentInput: "<html></html>",
            SourceUrl: "https://example.com",
            ToolUsed: "axe-core",
            Status: "success",
            SummaryResult: "Analysis completed",
            ResultJson: "{}",
            DurationMs: 100,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 0,
            AxeNeedsReview: 0,
            AxeRecommendations: 0,
            AxePasses: 5,
            AxeIncomplete: 0,
            AxeInapplicable: 0,
            EaViolations: 0,
            EaNeedsReview: 0,
            EaRecommendations: 0,
            EaPasses: 5,
            EaIncomplete: 0,
            EaInapplicable: 0
        );

        var expectedResult = CreateSampleAnalysisDto(100, 5);

        _ = _mockService.Setup(s => s.CreateAsync(It.Is<AnalysisCreateDto>(dto => dto.UserId == 5)))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        _ = result.Should().BeOfType<CreatedAtActionResult>();
        _mockService.Verify(s => s.CreateAsync(It.Is<AnalysisCreateDto>(dto => dto.UserId == 5)), Times.Once);
    }

    // ==================== Edge Cases - GetByDate with Date Range ====================

    [Fact]
    public async Task GetByDate_WithDateRange_ShouldCallGetByDateRangeAsync()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Today.AddDays(-7);
        var to = DateTime.Today;
        var analyses = new[] { CreateSampleAnalysisDto(1, userId) };

        _ = _mockService.Setup(s => s.GetByDateRangeAsync(userId, from, to))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetByDate(userId, DateTime.Today, from, to);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetByDateRangeAsync(userId, from, to), Times.Once);
        _mockService.Verify(s => s.GetByDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task GetByDate_WithoutDateRange_ShouldCallGetByDateAsync()
    {
        // Arrange
        var userId = 1;
        var date = DateTime.Today;
        var analyses = new[] { CreateSampleAnalysisDto(1, userId) };

        _ = _mockService.Setup(s => s.GetByDateAsync(userId, date))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetByDate(userId, date);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetByDateAsync(userId, date), Times.Once);
        _mockService.Verify(s => s.GetByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    // ==================== Edge Cases - GetByTool ====================

    [Fact]
    public async Task GetByTool_WithValidTool_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var toolUsed = "axe-core";
        var analyses = new[] { CreateSampleAnalysisDto(1, userId) };

        _ = _mockService.Setup(s => s.GetByToolAsync(userId, toolUsed))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetByTool(userId, toolUsed);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetByToolAsync(userId, toolUsed), Times.Once);
    }

    [Fact]
    public async Task GetByTool_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var toolUsed = "unknown-tool";
        _ = _mockService.Setup(s => s.GetByToolAsync(userId, toolUsed))
            .ReturnsAsync(Array.Empty<AnalysisDto>());

        // Act
        var result = await _controller.GetByTool(userId, toolUsed);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }

    // ==================== Edge Cases - GetByStatus ====================

    [Fact]
    public async Task GetByStatus_WithValidStatus_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var status = "success";
        var analyses = new[] { CreateSampleAnalysisDto(1, userId) };

        _ = _mockService.Setup(s => s.GetByStatusAsync(userId, status))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetByStatus(userId, status);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetByStatusAsync(userId, status), Times.Once);
    }

    [Fact]
    public async Task GetByStatus_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var status = "failed";
        _ = _mockService.Setup(s => s.GetByStatusAsync(userId, status))
            .ReturnsAsync(Array.Empty<AnalysisDto>());

        // Act
        var result = await _controller.GetByStatus(userId, status);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
    }

    // ==================== Edge Cases - Delete ====================

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var analysisId = 1;
        _ = _mockService.Setup(s => s.DeleteAsync(analysisId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(analysisId);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.DeleteAsync(analysisId), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenAnalysisNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var analysisId = 999;
        _ = _mockService.Setup(s => s.DeleteAsync(analysisId))
            .ThrowsAsync(new InvalidOperationException("Analysis not found"));

        // Act
        var result = await _controller.Delete(analysisId);

        // Assert
        _ = result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ==================== Edge Cases - Create Exception Handling ====================

    [Fact]
    public async Task Create_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        _ = _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        var createDto = CreateSampleCreateDto();

        _ = _mockService.Setup(s => s.CreateAsync(It.IsAny<AnalysisCreateDto>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(createDto));
    }

    private static AnalysisCreateDto CreateSampleCreateDto()
    {
        return new AnalysisCreateDto(
            UserId: 1,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "html",
            ContentInput: "<html></html>",
            SourceUrl: "https://example.com",
            ToolUsed: "axe-core",
            Status: "success",
            SummaryResult: "Analysis completed",
            ResultJson: "{}",
            DurationMs: 100,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 0,
            AxeNeedsReview: 0,
            AxeRecommendations: 0,
            AxePasses: 5,
            AxeIncomplete: 0,
            AxeInapplicable: 0,
            EaViolations: 0,
            EaNeedsReview: 0,
            EaRecommendations: 0,
            EaPasses: 5,
            EaIncomplete: 0,
            EaInapplicable: 0
        );
    }

    private static AnalysisDto CreateSampleAnalysisDto(int id, int userId)
    {
        return new AnalysisDto(
            Id: id,
            UserId: userId,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "html",
            ContentInput: "<html></html>",
            SourceUrl: "https://example.com",
            ToolUsed: "axe-core",
            Status: "success",
            SummaryResult: "Analysis completed",
            ResultJson: "{}",
            DurationMs: 100,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 0,
            AxeNeedsReview: 0,
            AxeRecommendations: 0,
            AxePasses: 5,
            AxeIncomplete: 0,
            AxeInapplicable: 0,
            EaViolations: 0,
            EaNeedsReview: 0,
            EaRecommendations: 0,
            EaPasses: 5,
            EaIncomplete: 0,
            EaInapplicable: 0,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );
    }
}
