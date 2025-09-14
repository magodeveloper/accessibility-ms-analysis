using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Analysis.Api.Controllers;
using Analysis.Application.Services.Analysis;
using Analysis.Application.Dtos;

namespace Analysis.Tests.Controllers
{
    public class AnalysisControllerTests
    {
        private readonly Mock<IAnalysisService> _mockService;
        private readonly AnalysisController _controller;

        public AnalysisControllerTests()
        {
            _mockService = new Mock<IAnalysisService>();
            _controller = new AnalysisController(_mockService.Object);

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

            _mockService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(analyses);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
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

            _mockService.Setup(s => s.GetByUserIdAsync(userId))
                .ReturnsAsync(analyses);

            // Act
            var result = await _controller.GetByUser(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var analysisId = 1;
            var analysis = CreateSampleAnalysisDto(analysisId, 1);

            _mockService.Setup(s => s.GetByIdAsync(analysisId))
                .ReturnsAsync(analysis);

            // Act
            var result = await _controller.GetById(analysisId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var analysisId = 999;
            _mockService.Setup(s => s.GetByIdAsync(analysisId))
                .ReturnsAsync((AnalysisDto?)null);

            // Act
            var result = await _controller.GetById(analysisId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
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

            _mockService.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().NotBeNull();
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

            _mockService.Setup(s => s.GetByDateAsync(userId, date))
                .ReturnsAsync(analyses);

            // Act
            var result = await _controller.GetByDate(userId, date);

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
                .ReturnsAsync(Array.Empty<AnalysisDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByUser_WhenEmpty_ShouldReturnOk()
        {
            // Arrange
            var userId = 1;
            _mockService.Setup(s => s.GetByUserIdAsync(userId))
                .ReturnsAsync(Array.Empty<AnalysisDto>());

            // Act
            var result = await _controller.GetByUser(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByDate_WhenEmpty_ShouldReturnOk()
        {
            // Arrange
            var userId = 1;
            var date = DateTime.Today;
            _mockService.Setup(s => s.GetByDateAsync(userId, date))
                .ReturnsAsync(Array.Empty<AnalysisDto>());

            // Act
            var result = await _controller.GetByDate(userId, date);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
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
}