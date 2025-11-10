using Analysis.Application;
using Analysis.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.UserContext;

namespace Analysis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorController(IErrorService service, IUserContext userContext) : ControllerBase
{
    private readonly IErrorService _service = service;
    private readonly IUserContext _userContext = userContext;

    /// <summary>
    /// Obtiene un error por su ID.
    /// </summary>
    /// <response code="200">Error encontrado</response>
    /// <response code="404">No se encontró el error</response>
    [HttpGet("{id}")]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(ErrorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { error = Localization.Get("Error_ErrorNotFound", lang) });
        }

        return Ok(new { error = result, message = Localization.Get("Success_ErrorFound", lang) });
    }

    /// <summary>
    /// Obtiene todos los errores asociados a un resultado.
    /// </summary>
    /// <response code="200">Lista de errores</response>
    /// <response code="404">No se encontraron errores</response>
    [HttpGet("by-result")]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), 200)]
    public async Task<IActionResult> GetByResultId([FromQuery] int resultId)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetByResultIdAsync(resultId);
        return Ok(new { errors = result, message = Localization.Get("Success_ErrorsByResult", lang) });
    }

    /// <summary>
    /// Obtiene todos los errores.
    /// </summary>
    /// <response code="200">Lista de todos los errores</response>
    /// <response code="404">No se encontraron errores</response>
    [HttpGet]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.GetAllAsync();
        return Ok(new { errors = result, message = Localization.Get("Success_ListErrors", lang) });
    }

    /// <summary>
    /// Crea un nuevo error.
    /// </summary>
    /// <response code="201">Error creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">No se encontró el error</response>
    [HttpPost]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(ErrorDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ErrorCreateDto dto)
    {
        // NOTA: La autenticación ya fue validada por el Gateway
        // El middleware necesita poder guardar errores a través del Gateway
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        var lang = LanguageHelper.GetRequestLanguage(Request);
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { message = Localization.Get("Success_ErrorCreated", lang), data = result });
    }

    /// <summary>
    /// Elimina un error por su ID.
    /// </summary>
    /// <response code="200">Error eliminado</response>
    /// <response code="404">No se encontró el error</response>
    [HttpDelete("{id}")]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = Localization.Get("Success_ErrorDeleted", lang) });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = Localization.Get("Error_ErrorNotFound", lang) });
        }
    }

    /// <summary>
    /// Elimina todos los errores.
    /// </summary>
    /// <response code="200">Todos los errores eliminados exitosamente</response>
    [HttpDelete("all")]
    [Authorize] // Requiere autenticación
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> DeleteAll()
    {
        var lang = LanguageHelper.GetRequestLanguage(Request);
        await _service.DeleteAllAsync();
        return Ok(new { message = Localization.Get("Success_AllErrorsDeleted", lang) });
    }
}
