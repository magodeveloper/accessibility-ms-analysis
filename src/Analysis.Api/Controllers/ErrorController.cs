using Analysis.Application;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Error;

namespace Analysis.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly IErrorService _service;
        public ErrorController(IErrorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene un error por su ID.
        /// </summary>
        /// <response code="200">Error encontrado</response>
        /// <response code="404">No se encontró el error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ErrorDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.GetByIdAsync(id);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            if (result == null)
                return NotFound(new { error = Localization.Get("Error_ErrorNotFound", lang) });
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todos los errores asociados a un resultado.
        /// </summary>
        /// <response code="200">Lista de errores</response>
        [HttpGet("by-result")]
        [ProducesResponseType(typeof(IEnumerable<ErrorDto>), 200)]
        public async Task<IActionResult> GetByResultId([FromQuery] int resultId)
        {
            var result = await _service.GetByResultIdAsync(resultId);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todos los errores.
        /// </summary>
        /// <response code="200">Lista de todos los errores</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ErrorDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo error.
        /// </summary>
        /// <response code="201">Error creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost]
        [ProducesResponseType(typeof(ErrorDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] ErrorCreateDto dto, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.CreateAsync(dto);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, new { message = Localization.Get("Success_ErrorCreated", lang), data = result });
        }

        /// <summary>
        /// Elimina un error por su ID.
        /// </summary>
        /// <response code="204">Eliminado exitosamente</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}