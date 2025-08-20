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
            var dto = new
            {
                UserId = 1,
                DateAnalysis = System.DateTime.UtcNow,
                ContentType = "html",
                ContentInput = "{ }",
                SourceUrl = "https://example.com",
                ToolUsed = "axe-core",
                Status = "pending",
                SummaryResult = "ok",
                ResultJson = "{}",
                ErrorMessage = "",
                DurationMs = 100,
                WcagVersion = "2.1",
                WcagLevel = "AA",
                AxeViolations = 0,
                AxeNeedsReview = 0,
                AxeRecommendations = 0,
                AxePasses = 0,
                AxeIncomplete = 0,
                AxeInapplicable = 0,
                EaViolations = 0,
                EaNeedsReview = 0,
                EaRecommendations = 0,
                EaPasses = 0,
                EaIncomplete = 0,
                EaInapplicable = 0
            };
            var createResp = await client.PostAsJsonAsync("/api/analysis", dto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            var data = created.GetProperty("data");
            int id = data.GetProperty("id").GetInt32();

            var getResp = await client.GetAsync($"/api/analysis/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task Create_And_Get_Result()
        {
            var client = _factory.CreateClient();
            // Primero crear un análisis válido
            var analysisDto = new
            {
                UserId = 1,
                DateAnalysis = System.DateTime.UtcNow,
                ContentType = "html",
                ContentInput = "{ }",
                SourceUrl = "https://example.com",
                ToolUsed = "axe-core",
                Status = "pending",
                SummaryResult = "ok",
                ResultJson = "{}",
                ErrorMessage = "",
                DurationMs = 100,
                WcagVersion = "2.1",
                WcagLevel = "AA",
                AxeViolations = 0,
                AxeNeedsReview = 0,
                AxeRecommendations = 0,
                AxePasses = 0,
                AxeIncomplete = 0,
                AxeInapplicable = 0,
                EaViolations = 0,
                EaNeedsReview = 0,
                EaRecommendations = 0,
                EaPasses = 0,
                EaIncomplete = 0,
                EaInapplicable = 0
            };
            var analysisResp = await client.PostAsJsonAsync("/api/analysis", analysisDto);
            var analysis = await analysisResp.Content.ReadFromJsonAsync<JsonElement>();
            var analysisData = analysis.GetProperty("data");
            int analysisId = analysisData.GetProperty("id").GetInt32();

            var resultDto = new { AnalysisId = analysisId, Description = "desc", Level = "A", Severity = "low", WcagCriterion = "1.1.1", WcagCriterionId = 1 };
            var createResp = await client.PostAsJsonAsync("/api/result", resultDto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var getResp = await client.GetAsync($"/api/result/by-analysis?analysisId={analysisId}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task Create_And_Get_Error()
        {
            var client = _factory.CreateClient();
            // Crear análisis válido
            var analysisDto = new
            {
                UserId = 1,
                DateAnalysis = System.DateTime.UtcNow,
                ContentType = "html",
                ContentInput = "{ }",
                SourceUrl = "https://example.com",
                ToolUsed = "axe-core",
                Status = "pending",
                SummaryResult = "ok",
                ResultJson = "{}",
                ErrorMessage = "",
                DurationMs = 100,
                WcagVersion = "2.1",
                WcagLevel = "AA",
                AxeViolations = 0,
                AxeNeedsReview = 0,
                AxeRecommendations = 0,
                AxePasses = 0,
                AxeIncomplete = 0,
                AxeInapplicable = 0,
                EaViolations = 0,
                EaNeedsReview = 0,
                EaRecommendations = 0,
                EaPasses = 0,
                EaIncomplete = 0,
                EaInapplicable = 0
            };
            var analysisResp = await client.PostAsJsonAsync("/api/analysis", analysisDto);
            var analysis = await analysisResp.Content.ReadFromJsonAsync<JsonElement>();
            var analysisData = analysis.GetProperty("data");
            int analysisId = analysisData.GetProperty("id").GetInt32();

            var resultDto = new { AnalysisId = analysisId, Description = "desc", Level = "A", Severity = "low", WcagCriterion = "1.1.1", WcagCriterionId = 1 };
            var resultResp = await client.PostAsJsonAsync("/api/result", resultDto);
            var result = await resultResp.Content.ReadFromJsonAsync<JsonElement>();
            var resultData = result.GetProperty("data");
            int resultId = resultData.GetProperty("id").GetInt32();

            var errorDto = new { ResultId = resultId, Description = "error", ErrorCode = "E1", Location = "loc", WcagCriterionId = 1, Message = "msg", Code = "C1" };
            var createResp = await client.PostAsJsonAsync("/api/error", errorDto);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            var errorData = created.GetProperty("data");
            int id = errorData.GetProperty("id").GetInt32();

            var getResp = await client.GetAsync($"/api/error/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task Create_Analysis_InvalidData_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var dto = new { UserId = 0, ToolUsed = "", Status = "", DateAnalysis = System.DateTime.UtcNow, ContentType = "", ContentInput = "", SourceUrl = "", WcagVersion = "", WcagLevel = "" };
            var createResp = await client.PostAsJsonAsync("/api/analysis", dto);
            Assert.Equal(HttpStatusCode.BadRequest, createResp.StatusCode);
            var content = await createResp.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;
            Assert.True(json.TryGetProperty("errors", out var errors));
            string[] campos = new[] { "UserId", "ToolUsed", "Status", "ContentType", "ContentInput", "SourceUrl", "WcagVersion", "WcagLevel" };
            var errorProps = errors.EnumerateObject().Select(p => p.Name.ToLowerInvariant()).ToHashSet();
            foreach (var campo in campos)
            {
                Assert.Contains(campo.ToLowerInvariant(), errorProps);
            }
        }
    }
}