using Xunit;
using System.Net;
using Analysis.Api;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Analysis.Tests
{
    public class AnalysisApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AnalysisApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create_And_Get_Analysis()
        {
            var client = _factory.CreateClient();
            var dto = new { UserId = 1, ToolUsed = "axe", Status = "completed", Date = System.DateTime.UtcNow };
            var createResp = await client.PostAsJsonAsync("/api/analysis", dto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.TryGetProperty("id", out var idProp));
            int id = idProp.GetInt32();

            var getResp = await client.GetAsync($"/api/analysis/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task Create_And_Get_Result()
        {
            var client = _factory.CreateClient();
            // Primero crear un análisis
            var analysisDto = new { UserId = 1, ToolUsed = "axe", Status = "completed", Date = System.DateTime.UtcNow };
            var analysisResp = await client.PostAsJsonAsync("/api/analysis", analysisDto);
            var analysis = await analysisResp.Content.ReadFromJsonAsync<JsonElement>();
            int analysisId = analysis.GetProperty("id").GetInt32();

            var resultDto = new { AnalysisId = analysisId, Score = 95, Summary = "Sin errores críticos" };
            var createResp = await client.PostAsJsonAsync("/api/result", resultDto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.TryGetProperty("id", out var idProp));
            int id = idProp.GetInt32();

            var getResp = await client.GetAsync($"/api/result/by-analysis?analysisId={analysisId}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task Create_And_Get_Error()
        {
            var client = _factory.CreateClient();
            // Crear análisis y resultado
            var analysisDto = new { UserId = 1, ToolUsed = "axe", Status = "completed", Date = System.DateTime.UtcNow };
            var analysisResp = await client.PostAsJsonAsync("/api/analysis", analysisDto);
            var analysis = await analysisResp.Content.ReadFromJsonAsync<JsonElement>();
            int analysisId = analysis.GetProperty("id").GetInt32();

            var resultDto = new { AnalysisId = analysisId, Score = 80, Summary = "Con errores menores" };
            var resultResp = await client.PostAsJsonAsync("/api/result", resultDto);
            var result = await resultResp.Content.ReadFromJsonAsync<JsonElement>();
            int resultId = result.GetProperty("id").GetInt32();

            var errorDto = new { ResultId = resultId, Message = "Falta atributo alt en imagen", Code = "IMG_ALT" };
            var createResp = await client.PostAsJsonAsync("/api/error", errorDto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.TryGetProperty("id", out var idProp));
            int id = idProp.GetInt32();

            var getResp = await client.GetAsync($"/api/error/by-result?resultId={resultId}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task Create_Analysis_InvalidData_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            // Falta UserId y ToolUsed vacío
            var dto = new { UserId = 0, ToolUsed = "", Status = "", Date = System.DateTime.UtcNow };
            var createResp = await client.PostAsJsonAsync("/api/analysis", dto);
            Assert.Equal(HttpStatusCode.BadRequest, createResp.StatusCode);
            var content = await createResp.Content.ReadAsStringAsync();
            Assert.Contains("UserId", content);
            Assert.Contains("ToolUsed", content);
            Assert.Contains("Status", content);
        }
    }
}