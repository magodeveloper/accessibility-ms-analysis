using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services;

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
        /// Obtiene análisis por fecha y usuario.
        /// </summary>
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate([FromQuery] int userId, [FromQuery] DateTime date)
        {
            var result = await _service.GetByDateAsync(userId, date);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene análisis por herramienta y usuario.
        /// </summary>
        [HttpGet("by-tool")]
        public async Task<IActionResult> GetByTool([FromQuery] int userId, [FromQuery] string toolUsed)
        {
            var result = await _service.GetByToolAsync(userId, toolUsed);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene análisis por estado y usuario.
        /// </summary>
        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] int userId, [FromQuery] string status)
        {
            var result = await _service.GetByStatusAsync(userId, status);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un análisis por ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.GetByIdAsync(id);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            if (result == null)
                return NotFound(new { error = Analysis.Api.Localization.Get("Error_AnalysisNotFound", lang) });
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo análisis.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AnalysisDto dto, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.CreateAsync(dto);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new
            {
                message = Analysis.Api.Localization.Get("Success_AnalysisCreated", lang),
                data = result
            });
        }

        /// <summary>
        /// Elimina un análisis por ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}