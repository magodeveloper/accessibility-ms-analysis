using Analysis.Application;
using Analysis.Api.Helpers;
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
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetAllAsync();
            return Ok(new { analyses = result, message = Localization.Get("Success_ListAnalysis", lang) });
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
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByUser", lang) });
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
            var lang = LanguageHelper.GetRequestLanguage(Request);
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

        /// <summary>
        /// Obtiene análisis por herramienta y usuario.
        /// </summary>
        /// <response code="200">Lista de análisis encontrados</response>
        /// <response code="404">No se encontraron análisis</response>
        [HttpGet("by-tool")]
        [ProducesResponseType(typeof(IEnumerable<AnalysisDto>), 200)]
        public async Task<IActionResult> GetByTool([FromQuery] int userId, [FromQuery] string toolUsed)
        {
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetByToolAsync(userId, toolUsed);
            return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByTool", lang) });
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
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetByStatusAsync(userId, status);
            return Ok(new { analyses = result, message = Localization.Get("Success_AnalysesByStatus", lang) });
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
            var lang = LanguageHelper.GetRequestLanguage(Request);
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { error = Localization.Get("Error_AnalysisNotFound", lang) });
            return Ok(new { analysis = result, message = Localization.Get("Success_AnalysisFound", lang) });
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
            var result = await _service.CreateAsync(dto);
            var lang = LanguageHelper.GetRequestLanguage(Request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new
            {
                message = Localization.Get("Success_AnalysisCreated", lang),
                data = result
            });
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
            return Ok(new { message = Localization.Get("Success_AllAnalysisDeleted", lang) });
        }
    }
}