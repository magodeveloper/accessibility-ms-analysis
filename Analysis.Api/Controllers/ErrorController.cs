using Microsoft.AspNetCore.Mvc;
using Analysis.Application.Dtos;
using Analysis.Application.Services;

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.GetByIdAsync(id);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            if (result == null)
                return NotFound(new { error = Analysis.Api.Localization.Get("Error_ErrorNotFound", lang) });
            return Ok(result);
        }

        [HttpGet("by-result")]
        public async Task<IActionResult> GetByResultId([FromQuery] int resultId)
        {
            var result = await _service.GetByResultIdAsync(resultId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ErrorDto dto, [FromHeader(Name = "Accept-Language")] string acceptLanguage = "es")
        {
            var result = await _service.CreateAsync(dto);
            var lang = acceptLanguage?.Split(',')[0] ?? "es";
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, new { message = Analysis.Api.Localization.Get("Success_ErrorCreated", lang), data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}