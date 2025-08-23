using Analysis.Application;
using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services.Analysis;

namespace Analysis.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _service;
        public AnalysisController(IAnalysisService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los análisis.
        /// </summary>
        /// <response code="200">Lista de análisis</response>
        /// <response code="404">No se encontraron análisis</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
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
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene análisis por fecha y usuario.
        /// </summary>
        /// <response code="200">Lista de análisis encontrados</response>
        /// <response code="404">No se encontraron análisis</response>
        [HttpGet("by-date")]
        [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
        public async Task<IActionResult> GetByDate([FromQuery] int userId, [FromQuery] DateTime date)
        {
            var result = await _service.GetByDateAsync(userId, date);
            return Ok(result);
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
            var result = await _service.GetByToolAsync(userId, toolUsed);
            return Ok(result);
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
            var result = await _service.GetByStatusAsync(userId, status);
            return Ok(result);
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
        public async Task<IActionResult> GetById(int id, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.GetByIdAsync(id);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            if (result == null)
                return NotFound(new { error = Localization.Get("Error_AnalysisNotFound", lang) });
            return Ok(result);
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
        public async Task<IActionResult> Create([FromBody] AnalysisCreateDto dto, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.CreateAsync(dto);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new
            {
                message = Localization.Get("Success_AnalysisCreated", lang),
                data = result
            });
        }

        /// <summary>
        /// Elimina un análisis por ID.
        /// </summary>
        /// <response code="204">Eliminado exitosamente</response>
        /// <response code="404">No se encontró el análisis</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Elimina todos los análisis.
        /// </summary>
        /// <response code="204">Todos los análisis eliminados exitosamente</response>
        [HttpDelete("all")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteAll()
        {
            await _service.DeleteAllAsync();
            return NoContent();
        }
    }
}