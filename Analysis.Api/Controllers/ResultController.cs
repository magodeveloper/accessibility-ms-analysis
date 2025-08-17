using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services;

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.GetByIdAsync(id);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            if (result == null)
                return NotFound(new { error = Analysis.Api.Localization.Get("Error_ResultNotFound", lang) });
            return Ok(result);
        }

        [HttpGet("by-analysis")]
        public async Task<IActionResult> GetByAnalysisId([FromQuery] int analysisId)
        {
            var result = await _service.GetByAnalysisIdAsync(analysisId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResultDto dto, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.CreateAsync(dto);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, new { message = Analysis.Api.Localization.Get("Success_ResultCreated", lang), data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}