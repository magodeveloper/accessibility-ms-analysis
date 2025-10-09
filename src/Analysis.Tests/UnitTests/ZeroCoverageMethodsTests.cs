using Moq;
using FluentAssertions;
using Analysis.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.Analysis;
using Analysis.Application.Services.UserContext;

namespace Analysis.Tests.UnitTests;

/// <summary>
/// Tests específicos para métodos con 0% de cobertura en Analysis.Api
/// </summary>
public class ZeroCoverageMethodsTests
{
    private readonly Mock<IAnalysisService> _mockAnalysisService;
    private readonly Mock<IErrorService> _mockErrorService;
    private readonly Mock<IResultService> _mockResultService;

    public ZeroCoverageMethodsTests()
    {
        _mockAnalysisService = new Mock<IAnalysisService>();
        _mockErrorService = new Mock<IErrorService>();
        _mockResultService = new Mock<IResultService>();
    }

    #region AnalysisController - GetByTool Tests (0% coverage)

    [Fact]
    public async Task AnalysisController_GetByTool_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var userId = 1;
        var toolUsed = "TestTool";

        _ = _mockAnalysisService.Setup(s => s.GetByToolAsync(userId, toolUsed))
            .ReturnsAsync(new List<AnalysisDto>());

        // Act
        var result = await controller.GetByTool(userId, toolUsed);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockAnalysisService.Verify(s => s.GetByToolAsync(userId, toolUsed), Times.Once);
    }

    [Fact]
    public async Task AnalysisController_GetByTool_WithNullTool_ShouldHandleCorrectly()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var userId = 1;
        string toolUsed = ""; // En lugar de null para evitar warnings

        _ = _mockAnalysisService.Setup(s => s.GetByToolAsync(userId, toolUsed))
            .ReturnsAsync(new List<AnalysisDto>());

        // Act
        var result = await controller.GetByTool(userId, toolUsed);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockAnalysisService.Verify(s => s.GetByToolAsync(userId, toolUsed), Times.Once);
    }

    [Fact]
    public async Task AnalysisController_GetByTool_WithException_ShouldThrow()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var userId = 1;
        var toolUsed = "TestTool";

        _ = _mockAnalysisService.Setup(s => s.GetByToolAsync(userId, toolUsed))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        _ = await Assert.ThrowsAsync<Exception>(() => controller.GetByTool(userId, toolUsed));
        _mockAnalysisService.Verify(s => s.GetByToolAsync(userId, toolUsed), Times.Once);
    }

    #endregion

    #region AnalysisController - GetByStatus Tests (0% coverage)

    [Fact]
    public async Task AnalysisController_GetByStatus_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var userId = 1;
        var status = "Completed";

        _ = _mockAnalysisService.Setup(s => s.GetByStatusAsync(userId, status))
            .ReturnsAsync(new List<AnalysisDto>());

        // Act
        var result = await controller.GetByStatus(userId, status);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockAnalysisService.Verify(s => s.GetByStatusAsync(userId, status), Times.Once);
    }

    [Fact]
    public async Task AnalysisController_GetByStatus_WithEmptyStatus_ShouldHandleCorrectly()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var userId = 1;
        string status = ""; // En lugar de null para evitar warnings

        _ = _mockAnalysisService.Setup(s => s.GetByStatusAsync(userId, status))
            .ReturnsAsync(new List<AnalysisDto>());

        // Act
        var result = await controller.GetByStatus(userId, status);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockAnalysisService.Verify(s => s.GetByStatusAsync(userId, status), Times.Once);
    }

    [Fact]
    public async Task AnalysisController_GetByStatus_WithException_ShouldThrow()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var userId = 1;
        var status = "InProgress";

        _ = _mockAnalysisService.Setup(s => s.GetByStatusAsync(userId, status))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        _ = await Assert.ThrowsAsync<Exception>(() => controller.GetByStatus(userId, status));
        _mockAnalysisService.Verify(s => s.GetByStatusAsync(userId, status), Times.Once);
    }

    #endregion

    #region AnalysisController - Delete Tests (0% coverage)

    [Fact]
    public async Task AnalysisController_Delete_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var id = 1;

        _ = _mockAnalysisService.Setup(s => s.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.Delete(id);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockAnalysisService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task AnalysisController_Delete_WithException_ShouldThrow()
    {
        // Arrange
        var controller = CreateAnalysisController();
        var id = 1;

        _ = _mockAnalysisService.Setup(s => s.DeleteAsync(id))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        _ = await Assert.ThrowsAsync<Exception>(() => controller.Delete(id));
        _mockAnalysisService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    #endregion

    #region ErrorController - Delete Tests (0% coverage)

    [Fact]
    public async Task ErrorController_Delete_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var controller = CreateErrorController();
        var id = 1;

        _ = _mockErrorService.Setup(s => s.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.Delete(id);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockErrorService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task ErrorController_Delete_WithException_ShouldThrow()
    {
        // Arrange
        var controller = CreateErrorController();
        var id = 1;

        _ = _mockErrorService.Setup(s => s.DeleteAsync(id))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        _ = await Assert.ThrowsAsync<Exception>(() => controller.Delete(id));
        _mockErrorService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    #endregion

    #region ResultController - Delete Tests (0% coverage)

    [Fact]
    public async Task ResultController_Delete_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var controller = CreateResultController();
        var id = 1;

        _ = _mockResultService.Setup(s => s.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.Delete(id);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockResultService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task ResultController_Delete_WithException_ShouldThrow()
    {
        // Arrange
        var controller = CreateResultController();
        var id = 1;

        _ = _mockResultService.Setup(s => s.DeleteAsync(id))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        _ = await Assert.ThrowsAsync<Exception>(() => controller.Delete(id));
        _mockResultService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    #endregion

    #region ResultController - GetById Tests (0% coverage)

    [Fact]
    public async Task ResultController_GetById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var controller = CreateResultController();
        var id = 1;

        // Crear un ResultDto válido
        var resultDto = new ResultDto(
            Id: 1,
            AnalysisId: 1,
            WcagCriterionId: 1,
            WcagCriterion: "1.1.1",
            Level: "Error",
            Severity: "High",
            Description: "Test description",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );

        _ = _mockResultService.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(resultDto);

        // Act
        var result = await controller.GetById(id);

        // Assert
        _ = result.Should().BeOfType<OkObjectResult>();
        _mockResultService.Verify(s => s.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task ResultController_GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var controller = CreateResultController();
        var id = 999;

        _ = _mockResultService.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync((ResultDto?)null);

        // Act
        var result = await controller.GetById(id);

        // Assert
        _ = result.Should().BeOfType<NotFoundObjectResult>();
        _mockResultService.Verify(s => s.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task ResultController_GetById_WithException_ShouldThrow()
    {
        // Arrange
        var controller = CreateResultController();
        var id = 1;

        _ = _mockResultService.Setup(s => s.GetByIdAsync(id))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        _ = await Assert.ThrowsAsync<Exception>(() => controller.GetById(id));
        _mockResultService.Verify(s => s.GetByIdAsync(id), Times.Once);
    }

    #endregion

    #region Helper Methods

    private AnalysisController CreateAnalysisController()
    {
        var mockUserContext = new Mock<IUserContext>();
        _ = mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _ = mockUserContext.Setup(x => x.Role).Returns("user");
        _ = mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _ = mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        var controller = new AnalysisController(_mockAnalysisService.Object, mockUserContext.Object)
        {
            // Configurar HttpContext
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.Request.Headers["Accept-Language"] = "es";

        return controller;
    }

    private ErrorController CreateErrorController()
    {
        var mockUserContext = new Mock<IUserContext>();
        _ = mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _ = mockUserContext.Setup(x => x.Role).Returns("user");
        _ = mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _ = mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        var controller = new ErrorController(_mockErrorService.Object, mockUserContext.Object)
        {
            // Configurar HttpContext
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.Request.Headers["Accept-Language"] = "es";

        return controller;
    }

    private ResultController CreateResultController()
    {
        var mockUserContext = new Mock<IUserContext>();
        _ = mockUserContext.Setup(x => x.UserId).Returns(1);
        _ = mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _ = mockUserContext.Setup(x => x.Role).Returns("user");
        _ = mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _ = mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _ = mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        var controller = new ResultController(_mockResultService.Object, mockUserContext.Object)
        {
            // Configurar HttpContext
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.Request.Headers["Accept-Language"] = "es";

        return controller;
    }

    #endregion
}
