using Analysis.Application;
using Analysis.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.UserContext;

namespace Analysis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultController(IResultService service, IUserContext userContext) : ControllerBase
{
    private readonly IResultService _service = service;
    private readonly IUserContext _userContext = userContext;

    /// <summary>
    /// Obtiene resultados por nivel (level).
    /// </summary>
    /// <response code="200">Lista de resultados filtrados por nivel</response>
    /// <response code="404">No se encontraron resultados</response>
    [HttpGet("by-level")]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), 200)]
    public async Task<IActionResult> GetByLevel([FromQuery] string level)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetByLevelAsync(level);
        return Ok(new { results = result, message = Localization.Get("Success_ResultsByLevel", lang) });
    }

    /// <summary>
    /// Obtiene resultados por severidad (severity).
    /// </summary>
    /// <response code="200">Lista de resultados filtrados por severidad</response>
    /// <response code="404">No se encontraron resultados</response>
    [HttpGet("by-severity")]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), 200)]
    public async Task<IActionResult> GetBySeverity([FromQuery] string severity)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetBySeverityAsync(severity);
        return Ok(new { results = result, message = Localization.Get("Success_ResultsBySeverity", lang) });
    }

    /// <summary>
    /// Obtiene un resultado por su ID.
    /// </summary>
    /// <response code="200">Resultado encontrado</response>
    /// <response code="404">No se encontró el resultado</response>
    /// <response code="400">ID inválido</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResultDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { error = Localization.Get("Error_ResultNotFound", lang) });
        }

        return Ok(new { result, message = Localization.Get("Success_ResultFound", lang) });
    }

    /// <summary>
    /// Obtiene todos los resultados asociados a un análisis.
    /// </summary>
    /// <response code="200">Lista de resultados</response>
    /// <response code="404">No se encontraron resultados</response>
    [HttpGet("by-analysis")]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), 200)]
    public async Task<IActionResult> GetByAnalysisId([FromQuery] int analysisId)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetByAnalysisIdAsync(analysisId);
        return Ok(new { results = result, message = Localization.Get("Success_ResultsByAnalysis", lang) });
    }

    /// <summary>
    /// Obtiene todos los resultados.
    /// </summary>
    /// <response code="200">Lista de todos los resultados</response>
    /// <response code="404">No se encontraron resultados</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetAllAsync();
        return Ok(new { results = result, message = Localization.Get("Success_ListResults", lang) });
    }

    /// <summary>
    /// Crea un nuevo resultado.
    /// </summary>
    /// <response code="201">Resultado creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">No se encontró el resultado</response>
    [HttpPost]
    [ProducesResponseType(typeof(ResultDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ResultCreateDto dto)
    {
        // NOTA: La autenticación ya fue validada por el Gateway
        // El Gateway agrega X-User-* headers que el UserContext usa
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { message = Localization.Get("Success_ResultCreated", lang), data = result });
    }

    /// <summary>
    /// Elimina un resultado por su ID.
    /// </summary>
    /// <response code="200">Resultado eliminado</response>
    /// <response code="404">No se encontró el resultado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = Localization.Get("Success_ResultDeleted", lang) });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = Localization.Get("Error_ResultNotFound", lang) });
        }
    }

    /// <summary>
    /// Elimina todos los resultados.
    /// </summary>
    /// <response code="200">Todos los resultados eliminados exitosamente</response>
    [HttpDelete("all")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> DeleteAll()
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        await _service.DeleteAllAsync();
        return Ok(new { message = Localization.Get("Success_AllResultsDeleted", lang) });
    }
}
