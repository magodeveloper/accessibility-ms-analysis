using Xunit;
using System.Net;
using System.Text.Json;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Analysis.Api;

namespace Analysis.Tests
{
    public class AnalysisApiFullTests : IClassFixture<WebApplicationFactory<Program>>
    {
    private readonly WebApplicationFactory<Program> _factory;

    public AnalysisApiFullTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }


        [Fact]
        public async Task Analysis_CRUD()
        {
            var client = _factory.CreateClient();
            // Crear análisis con todos los campos requeridos
            var dto = new {
                UserId = 42,
                DateAnalysis = DateTime.UtcNow,
                ContentType = "text/html",
                ContentInput = "<html></html>",
                SourceUrl = "https://example.com",
                ToolUsed = "axe-core",
                Status = "pending",
                SummaryResult = "",
                ResultJson = "{}",
                ErrorMessage = "",
                DurationMs = 100,
                WcagVersion = "2.1",
                WcagLevel = "AA"
            };
            var createResp = await client.PostAsJsonAsync("/api/analysis", dto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            int id = created.GetProperty("data").GetProperty("id").GetInt32();

            // Consultar
            var getResp = await client.GetAsync($"/api/analysis/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            // Eliminar
            var delResp = await client.DeleteAsync($"/api/analysis/{id}");
            Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);
        }


        [Fact]
        public async Task Result_CRUD()
        {
            var client = _factory.CreateClient();
            // Crear análisis primero
            var analysisDto = new {
                UserId = 42,
                DateAnalysis = DateTime.UtcNow,
                ContentType = "text/html",
                ContentInput = "<html></html>",
                SourceUrl = "https://example.com",
                ToolUsed = "axe-core",
                Status = "completed",
                SummaryResult = "",
                ResultJson = "{}",
                ErrorMessage = "",
                DurationMs = 100,
                WcagVersion = "2.1",
                WcagLevel = "AA"
            };
            var analysisResp = await client.PostAsJsonAsync("/api/analysis", analysisDto);
            var analysis = await analysisResp.Content.ReadFromJsonAsync<JsonElement>();
            int analysisId = analysis.GetProperty("data").GetProperty("id").GetInt32();

            // Crear resultado con todos los campos requeridos
            var resultDto = new {
                AnalysisId = analysisId,
                WcagCriterionId = 1,
                WcagCriterion = "1.1.1",
                Level = "A",
                Severity = "error",
                Description = "Falta texto alternativo"
            };
            var createResp = await client.PostAsJsonAsync("/api/result", resultDto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            int id = created.GetProperty("data").GetProperty("id").GetInt32();

            // Consultar
            var getResp = await client.GetAsync($"/api/result/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            // Eliminar
            var delResp = await client.DeleteAsync($"/api/result/{id}");
            Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);
        }


        [Fact]
        public async Task Error_CRUD()
        {
            var client = _factory.CreateClient();
            // Crear análisis y resultado primero
            var analysisDto = new {
                UserId = 42,
                DateAnalysis = DateTime.UtcNow,
                ContentType = "text/html",
                ContentInput = "<html></html>",
                SourceUrl = "https://example.com",
                ToolUsed = "axe-core",
                Status = "completed",
                SummaryResult = "",
                ResultJson = "{}",
                ErrorMessage = "",
                DurationMs = 100,
                WcagVersion = "2.1",
                WcagLevel = "AA"
            };
            var analysisResp = await client.PostAsJsonAsync("/api/analysis", analysisDto);
            var analysis = await analysisResp.Content.ReadFromJsonAsync<JsonElement>();
            int analysisId = analysis.GetProperty("data").GetProperty("id").GetInt32();

            var resultDto = new {
                AnalysisId = analysisId,
                WcagCriterionId = 1,
                WcagCriterion = "1.1.1",
                Level = "A",
                Severity = "error",
                Description = "Falta texto alternativo"
            };
            var resultResp = await client.PostAsJsonAsync("/api/result", resultDto);
            var result = await resultResp.Content.ReadFromJsonAsync<JsonElement>();
            int resultId = result.GetProperty("data").GetProperty("id").GetInt32();

            // Crear error con todos los campos requeridos
            var errorDto = new {
                ResultId = resultId,
                WcagCriterionId = 1,
                ErrorCode = "WCAG-1.1.1",
                Description = "Falta texto alternativo en imagen",
                Location = "img#logo",
                Message = "Falta texto alternativo en imagen",
                Code = "WCAG-1.1.1"
            };
            var createResp = await client.PostAsJsonAsync("/api/error", errorDto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            int id = created.GetProperty("data").GetProperty("id").GetInt32();

            // Consultar
            var getResp = await client.GetAsync($"/api/error/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            // Eliminar
            var delResp = await client.DeleteAsync($"/api/error/{id}");
            Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);
        }
    }
}
