using Moq;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Analysis.Api.Controllers;
using Analysis.Application.Dtos;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Analysis.Application.Services.Composite;
using Analysis.Application.Services.UserContext;

namespace Analysis.Tests.Controllers;

/// <summary>
/// Tests unitarios para CompositeAnalysisController.
/// </summary>
public class CompositeAnalysisControllerTests
{
    private readonly Mock<ICompositeAnalysisService> _mockCompositeService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly CompositeAnalysisController _controller;

    public CompositeAnalysisControllerTests()
    {
        _mockCompositeService = new Mock<ICompositeAnalysisService>();
        _mockUserContext = new Mock<IUserContext>();
        _controller = new CompositeAnalysisController(
            _mockCompositeService.Object,
            _mockUserContext.Object
        );

        // Setup default HttpContext with headers
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.HttpContext.Request.Headers["Accept-Language"] = "es";
    }

    #region GetCompleteAnalysisById Tests

    [Fact]
    public async Task GetCompleteAnalysisById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var analysisId = 1;
        var userId = 123;
        var completeAnalysis = CreateSampleCompleteAnalysis(analysisId, userId);

        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(userId);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);
        _mockCompositeService
            .Setup(x => x.GetCompleteAnalysisByIdAsync(analysisId))
            .ReturnsAsync(completeAnalysis);

        // Act
        var result = await _controller.GetCompleteAnalysisById(analysisId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetCompleteAnalysisById_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _controller.GetCompleteAnalysisById(1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task GetCompleteAnalysisById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var analysisId = 999;
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(123);
        _mockCompositeService
            .Setup(x => x.GetCompleteAnalysisByIdAsync(analysisId))
            .ReturnsAsync((CompleteAnalysisDto?)null);

        // Act
        var result = await _controller.GetCompleteAnalysisById(analysisId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetCompleteAnalysisById_UserAccessingOthersAnalysis_ReturnsForbid()
    {
        // Arrange
        var analysisId = 1;
        var ownerUserId = 123;
        var requestingUserId = 456; // Different user
        var completeAnalysis = CreateSampleCompleteAnalysis(analysisId, ownerUserId);

        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(requestingUserId);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);
        _mockCompositeService
            .Setup(x => x.GetCompleteAnalysisByIdAsync(analysisId))
            .ReturnsAsync(completeAnalysis);

        // Act
        var result = await _controller.GetCompleteAnalysisById(analysisId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetCompleteAnalysisById_AdminAccessingOthersAnalysis_ReturnsOk()
    {
        // Arrange
        var analysisId = 1;
        var ownerUserId = 123;
        var adminUserId = 456;
        var completeAnalysis = CreateSampleCompleteAnalysis(analysisId, ownerUserId);

        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(adminUserId);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(true); // Admin can access
        _mockCompositeService
            .Setup(x => x.GetCompleteAnalysisByIdAsync(analysisId))
            .ReturnsAsync(completeAnalysis);

        // Act
        var result = await _controller.GetCompleteAnalysisById(analysisId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region GetCompleteAnalysesByUserId Tests

    [Fact]
    public async Task GetCompleteAnalysesByUserId_WithValidUserId_ReturnsOkResult()
    {
        // Arrange
        var userId = 123;
        var analyses = new List<CompleteAnalysisDto>
        {
            CreateSampleCompleteAnalysis(1, userId),
            CreateSampleCompleteAnalysis(2, userId)
        };

        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(userId);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);
        _mockCompositeService
            .Setup(x => x.GetCompleteAnalysesByUserIdAsync(userId))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetCompleteAnalysesByUserId(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserId_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _controller.GetCompleteAnalysesByUserId(123);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserId_UserAccessingOthersData_ReturnsForbid()
    {
        // Arrange
        var targetUserId = 123;
        var requestingUserId = 456;

        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(requestingUserId);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await _controller.GetCompleteAnalysesByUserId(targetUserId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserId_AdminAccessingOthersData_ReturnsOk()
    {
        // Arrange
        var targetUserId = 123;
        var adminUserId = 456;
        var analyses = new List<CompleteAnalysisDto>
        {
            CreateSampleCompleteAnalysis(1, targetUserId)
        };

        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(adminUserId);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(true);
        _mockCompositeService
            .Setup(x => x.GetCompleteAnalysesByUserIdAsync(targetUserId))
            .ReturnsAsync(analyses);

        // Act
        var result = await _controller.GetCompleteAnalysesByUserId(targetUserId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetCompleteAnalysesByUserId_WithNoAnalyses_ReturnsEmptyList()
    {
        // Arrange
        var userId = 123;
        var emptyList = new List<CompleteAnalysisDto>();

        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(userId);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);
        _mockCompositeService
            .Setup(x => x.GetCompleteAnalysesByUserIdAsync(userId))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetCompleteAnalysesByUserId(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region Helper Methods

    private static CompleteAnalysisDto CreateSampleCompleteAnalysis(int id, int userId)
    {
        var errors = new List<CompleteErrorDto>
        {
            new CompleteErrorDto(
                Id: 1,
                ResultId: 1,
                WcagCriterionId: 1,
                ErrorCode: "image-alt",
                Description: "Missing alt attribute",
                Location: "img.hero line 45",
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow
            )
        };

        var results = new List<CompleteResultDto>
        {
            new CompleteResultDto(
                Id: 1,
                AnalysisId: id,
                WcagCriterionId: 1,
                WcagCriterion: "1.1.1",
                Level: "A",
                Severity: "critical",
                Description: "Images must have alternate text",
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow,
                Errors: errors
            )
        };

        return new CompleteAnalysisDto(
            Id: id,
            UserId: userId,
            DateAnalysis: DateTime.UtcNow,
            ContentType: "url",
            ContentInput: null,
            SourceUrl: "https://example.com",
            ToolUsed: "axe",
            Status: "completed",
            SummaryResult: "10 violations found",
            ResultJson: "{}",
            DurationMs: 2500,
            WcagVersion: "2.1",
            WcagLevel: "AA",
            AxeViolations: 10,
            AxeNeedsReview: 5,
            AxeRecommendations: 2,
            AxePasses: 50,
            AxeIncomplete: 1,
            AxeInapplicable: 10,
            EaViolations: null,
            EaNeedsReview: null,
            EaRecommendations: null,
            EaPasses: null,
            EaIncomplete: null,
            EaInapplicable: null,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Results: results
        );
    }

    #endregion
}
