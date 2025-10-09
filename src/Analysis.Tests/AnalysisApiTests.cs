using System.Net;
using Analysis.Api;
using System.Text.Json;
using System.Net.Http.Json;
using Analysis.Tests.Infrastructure;

namespace Analysis.Tests;

public class AnalysisApiTests(TestWebApplicationFactory<Program> factory) : IClassFixture<TestWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory<Program> _factory = factory;

    public async Task InitializeAsync()
    {
        // Con bases de datos en memoria no necesitamos semilla de datos externos
        // Los datos se crearán en cada test según se necesiten
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Con bases de datos en memoria no necesitamos limpiar
        await Task.CompletedTask;
    }

    /// <summary>
    /// Helper method para crear un cliente HTTP con headers de usuario autenticado (simulando Gateway)
    /// </summary>
    private HttpClient CreateAuthenticatedClient(int userId = 1, string email = "test@test.com", string role = "user", string userName = "Test User")
    {
        // Usar el método del TestWebApplicationFactory que incluye X-Gateway-Secret
        return _factory.CreateAuthenticatedClient(userId, email, role, userName);
    }

    [Fact]
    public async Task Create_And_Get_Analysis()
    {
        var client = CreateAuthenticatedClient(userId: 1);
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
        var client = CreateAuthenticatedClient(userId: 1);
        client.DefaultRequestHeaders.Add("Accept-Language", "x"); // header corto para probar fallback seguro
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
        var client = CreateAuthenticatedClient(userId: 1);
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9"); // idioma válido
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
    public async Task Create_Result_With_Invalid_AcceptLanguage_Header_Does_Not_Fail()
    {
        var client = CreateAuthenticatedClient(userId: 1);
        client.DefaultRequestHeaders.Add("Accept-Language", ""); // vacío

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
        _ = analysisResp.EnsureSuccessStatusCode();
        var analysis = await analysisResp.Content.ReadFromJsonAsync<JsonElement>();
        int analysisId = analysis.GetProperty("data").GetProperty("id").GetInt32();

        var resultDto = new { AnalysisId = analysisId, Description = "desc", Level = "violation", Severity = "high", WcagCriterion = "1.1.1", WcagCriterionId = 1 };
        var createResp = await client.PostAsJsonAsync("/api/result", resultDto);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
    }

    [Fact]
    public async Task Create_Analysis_InvalidData_ReturnsBadRequest()
    {
        var client = CreateAuthenticatedClient(userId: 1);
        var dto = new { UserId = 0, ToolUsed = "", Status = "", DateAnalysis = System.DateTime.UtcNow, ContentType = "", ContentInput = "", SourceUrl = "", WcagVersion = "", WcagLevel = "" };
        var createResp = await client.PostAsJsonAsync("/api/analysis", dto);
        Assert.Equal(HttpStatusCode.BadRequest, createResp.StatusCode);
        var content = await createResp.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;
        Assert.True(json.TryGetProperty("errors", out var errors));
        string[] campos = ["UserId", "ToolUsed", "Status", "ContentType", "ContentInput", "SourceUrl", "WcagVersion", "WcagLevel"];
        var errorProps = errors.EnumerateObject().Select(p => p.Name.ToLowerInvariant()).ToHashSet();
        foreach (var campo in campos)
        {
            Assert.Contains(campo.ToLowerInvariant(), errorProps);
        }
    }
}
