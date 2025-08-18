using Analysis.Application;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Result;

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
        /// Obtiene un resultado por su ID.
        /// </summary>
        /// <response code="200">Resultado encontrado</response>
        /// <response code="404">No se encontró el resultado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResultDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.GetByIdAsync(id);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            if (result == null)
                return NotFound(new { error = Localization.Get("Error_ResultNotFound", lang) });
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todos los resultados asociados a un análisis.
        /// </summary>
        /// <response code="200">Lista de resultados</response>
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
        [HttpPost]
        [ProducesResponseType(typeof(ResultDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] ResultCreateDto dto, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.CreateAsync(dto);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, new { message = Localization.Get("Success_ResultCreated", lang), data = result });
        }

        /// <summary>
        /// Elimina un resultado por su ID.
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