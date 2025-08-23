using Analysis.Application;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Result;
using Analysis.Api.Helpers;

namespace Analysis.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly IResultService _service;
        public ResultController(IResultService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene resultados por nivel (level).
        /// </summary>
        /// <response code="200">Lista de resultados filtrados por nivel</response>
        /// <response code="404">No se encontraron resultados</response>
        [HttpGet("by-level")]
        [ProducesResponseType(typeof(IEnumerable<ResultDto>), 200)]
        public async Task<IActionResult> GetByLevel([FromQuery] string level)
        {
            var result = await _service.GetByLevelAsync(level);
            return Ok(result);
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
            var result = await _service.GetBySeverityAsync(severity);
            return Ok(result);
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
            var result = await _service.GetByIdAsync(id);
            var lang = LanguageHelper.GetRequestLanguage(Request);
            if (result == null)
                return NotFound(new { error = Localization.Get("Error_ResultNotFound", lang) });
            return Ok(result);
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
            var result = await _service.GetByAnalysisIdAsync(analysisId);
            return Ok(result);
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
            var result = await _service.GetAllAsync();
            return Ok(result);
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
            var result = await _service.CreateAsync(dto);
            var lang = LanguageHelper.GetRequestLanguage(Request);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, new { message = Localization.Get("Success_ResultCreated", lang), data = result });
        }

        /// <summary>
        /// Elimina un resultado por su ID.
        /// </summary>
        /// <response code="204">Eliminado exitosamente</response>
        /// <response code="404">No se encontró el resultado</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Elimina todos los resultados.
        /// </summary>
        /// <response code="204">Todos los resultados eliminados exitosamente</response>
        [HttpDelete("all")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteAll()
        {
            await _service.DeleteAllAsync();
            return NoContent();
        }
    }
}