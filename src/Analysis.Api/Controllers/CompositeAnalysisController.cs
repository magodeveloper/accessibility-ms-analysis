using Analysis.Api.Metrics;
using Analysis.Api.Helpers;
using Analysis.Application;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Analysis.Application.Services.Composite;
using Analysis.Application.Services.UserContext;

namespace Analysis.Api.Controllers;

/// <summary>
/// Controlador compuesto para obtener análisis completos con resultados y errores.
/// Requiere autenticación JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CompositeAnalysisController : ControllerBase
{
    private readonly ICompositeAnalysisService _compositeService;
    private readonly IUserContext _userContext;

    public CompositeAnalysisController(
        ICompositeAnalysisService compositeService,
        IUserContext userContext)
    {
        _compositeService = compositeService;
        _userContext = userContext;
    }

    /// <summary>
    /// Obtiene un análisis completo por ID incluyendo todos sus resultados y errores asociados.
    /// Requiere autenticación JWT.
    /// </summary>
    /// <param name="id">ID del análisis</param>
    /// <response code="200">Análisis completo encontrado</response>
    /// <response code="401">No autenticado</response>
    /// <response code="403">Sin permisos para acceder al análisis</response>
    /// <response code="404">No se encontró el análisis</response>
    [HttpGet("{id}")]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(CompleteAnalysisDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCompleteAnalysisById(int id)
    {
        using (AnalysisMetrics.MeasureQuery("get_complete_analysis_by_id"))
        {
            // Validar autenticación
            if (!_userContext.IsAuthenticated)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _compositeService.GetCompleteAnalysisByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { error = Localization.Get("Error_AnalysisNotFound", lang) });
            }

            // Validar que el usuario solo acceda a sus propios análisis (a menos que sea admin)
            if (!_userContext.IsAdmin && result.UserId != _userContext.UserId)
            {
                return Forbid(); // 403 Forbidden
            }

            AnalysisMetrics.RecordQuery("get_complete_analysis_by_id");
            return Ok(new
            {
                analysis = result,
                message = Localization.Get("Success_AnalysisFound", lang)
            });
        }
    }

    /// <summary>
    /// Obtiene todos los análisis completos de un usuario incluyendo resultados y errores.
    /// Requiere autenticación JWT.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <response code="200">Lista de análisis completos encontrados</response>
    /// <response code="401">No autenticado</response>
    /// <response code="403">Sin permisos para acceder a los análisis del usuario</response>
    /// <response code="404">No se encontraron análisis</response>
    [HttpGet("by-user")]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(IEnumerable<CompleteAnalysisDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCompleteAnalysesByUserId([FromQuery] int userId)
    {
        using (AnalysisMetrics.MeasureQuery("get_complete_analyses_by_user"))
        {
            // Validar autenticación
            if (!_userContext.IsAuthenticated)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            // Validar que el usuario solo acceda a sus propios análisis (a menos que sea admin)
            if (!_userContext.IsAdmin && userId != _userContext.UserId)
            {
                return Forbid(); // 403 Forbidden
            }

            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _compositeService.GetCompleteAnalysesByUserIdAsync(userId);

            AnalysisMetrics.RecordQuery("get_complete_analyses_by_user");
            return Ok(new
            {
                analyses = result,
                message = Localization.Get("Success_AnalysesByUser", lang)
            });
        }
    }
}
