using Analysis.Api.Metrics;
using Analysis.Api.Helpers;
using Analysis.Application;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Analysis;
using Analysis.Application.Services.UserContext;

namespace Analysis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController(IAnalysisService service, IUserContext userContext) : ControllerBase
{
    private readonly IAnalysisService _service = service;
    private readonly IUserContext _userContext = userContext;

    /// <summary>
    /// Obtiene todos los análisis.
    /// </summary>
    /// <response code="200">Lista de análisis</response>
    /// <response code="404">No se encontraron análisis</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        using (AnalysisMetrics.MeasureQuery("get_all"))
        {
            // Validar autenticación
            if (!_userContext.IsAuthenticated)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var lang = LanguageHelper.GetRequestLanguage(Request);
            // Filtrar automáticamente por userId del contexto autenticado
            var result = await _service.GetByUserIdAsync(_userContext.UserId);
            AnalysisMetrics.RecordQuery("get_all");
            return Ok(new { analyses = result, message = Localization.Get("Success_ListAnalysis", lang) });
        }
    }

    /// <summary>
    /// Obtiene análisis por usuario.
    /// </summary>
    /// <response code="200">Lista de análisis encontrados</response>
    /// <response code="404">No se encontraron análisis</response>
    [HttpGet("by-user")]
    [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
    public async Task<IActionResult> GetByUser([FromQuery] int userId)
    {
        using (AnalysisMetrics.MeasureQuery("get_by_user"))
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
            var result = await _service.GetByUserIdAsync(userId);
            AnalysisMetrics.RecordQuery("get_by_user");
            return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByUser", lang) });
        }
    }

    /// <summary>
    /// Obtiene análisis por fecha y usuario.
    /// </summary>
    /// <response code="200">Lista de análisis encontrados</response>
    /// <response code="404">No se encontraron análisis</response>
    [HttpGet("by-date")]
    [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
    public async Task<IActionResult> GetByDate(
        [FromQuery] int userId,
        [FromQuery] DateTime date,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        using (AnalysisMetrics.MeasureQuery("get_by_date"))
        {
            var lang = LanguageHelper.GetRequestLanguage(Request);
            AnalysisMetrics.RecordQuery("get_by_date");

            // Permitir parámetros opcionales from y to para rango
            if (from.HasValue && to.HasValue)
            {
                var result = await _service.GetByDateRangeAsync(userId, from.Value, to.Value);
                return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByDate", lang) });
            }
            else
            {
                var result = await _service.GetByDateAsync(userId, date);
                return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByDate", lang) });
            }
        }
    }

    /// <summary>
    /// Obtiene análisis por herramienta y usuario.
    /// </summary>
    /// <response code="200">Lista de análisis encontrados</response>
    /// <response code="404">No se encontraron análisis</response>
    [HttpGet("by-tool")]
    [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
    public async Task<IActionResult> GetByTool([FromQuery] int userId, [FromQuery] string toolUsed)
    {
        using (AnalysisMetrics.MeasureQuery("get_by_tool"))
        {
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetByToolAsync(userId, toolUsed);
            AnalysisMetrics.RecordQuery("get_by_tool");
            return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByTool", lang) });
        }
    }

    /// <summary>
    /// Obtiene análisis por estado y usuario.
    /// </summary>
    /// <response code="200">Lista de análisis encontrados</response>
    /// <response code="404">No se encontraron análisis</response>
    [HttpGet("by-status")]
    [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
    public async Task<IActionResult> GetByStatus([FromQuery] int userId, [FromQuery] string status)
    {
        using (AnalysisMetrics.MeasureQuery("get_by_status"))
        {
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetByStatusAsync(userId, status);
            AnalysisMetrics.RecordQuery("get_by_status");
            return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByStatus", lang) });
        }
    }

    /// <summary>
    /// Obtiene un análisis por ID.
    /// </summary>
    /// <response code="200">Análisis encontrado</response>
    /// <response code="404">No se encontró el análisis</response>
    /// <response code="400">ID inválido</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnalysisDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        using (AnalysisMetrics.MeasureQuery("get_by_id"))
        {
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetByIdAsync(id);
            AnalysisMetrics.RecordQuery("get_by_id");

            if (result == null)
            {
                return NotFound(new { error = Localization.Get("Error_AnalysisNotFound", lang) });
            }

            return Ok(new { analysis = result, message = Localization.Get("Success_AnalysisFound", lang) });
        }
    }

    /// <summary>
    /// Crea un nuevo análisis.
    /// </summary>
    /// <response code="201">Análisis creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">No se encontró el análisis</response>
    [HttpPost]
    [ProducesResponseType(typeof(AnalysisDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] AnalysisCreateDto dto)
    {
        // NOTA: La autenticación ya fue validada por el Gateway
        // El Gateway agrega X-User-* headers que el UserContext usa
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        // Override userId con el del contexto autenticado (ignorar el del body)
        // Si no hay usuario autenticado, usar el del DTO
        var dtoWithAuthUserId = _userContext.IsAuthenticated
            ? dto with { UserId = _userContext.UserId }
            : dto;

        using (AnalysisMetrics.MeasureAnalysis(dtoWithAuthUserId.ToolUsed.ToString()))
        {
            try
            {
                AnalysisMetrics.IncrementInProgress();
                var result = await _service.CreateAsync(dtoWithAuthUserId);
                AnalysisMetrics.RecordAnalysisPerformed(dtoWithAuthUserId.ToolUsed.ToString(), dtoWithAuthUserId.Status.ToString());

                var lang = LanguageHelper.GetRequestLanguage(Request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, new
                {
                    message = Localization.Get("Success_AnalysisCreated", lang),
                    data = result
                });
            }
            catch
            {
                AnalysisMetrics.RecordAnalysisPerformed(dtoWithAuthUserId.ToolUsed.ToString(), "failed");
                throw;
            }
            finally
            {
                AnalysisMetrics.DecrementInProgress();
            }
        }
    }

    /// <summary>
    /// Elimina un análisis por ID.
    /// </summary>
    /// <response code="200">Análisis eliminado</response>
    /// <response code="404">No se encontró el análisis</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        try
        {
            await _service.DeleteAsync(id);
            AnalysisMetrics.AnalysesDeletionsTotal.Inc();
            return Ok(new { message = Localization.Get("Success_AnalysisDeleted", lang) });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = Localization.Get("Error_AnalysisNotFound", lang) });
        }
    }

    /// <summary>
    /// Elimina todos los análisis.
    /// </summary>
    /// <response code="200">Todos los análisis eliminados exitosamente</response>
    [HttpDelete("all")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> DeleteAll()
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        await _service.DeleteAllAsync();
        AnalysisMetrics.AnalysesDeletionsTotal.Inc();
        return Ok(new { message = Localization.Get("Success_AllAnalysisDeleted", lang) });
    }
}
