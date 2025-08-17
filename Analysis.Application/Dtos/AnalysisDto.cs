namespace Analysis.Application.Dtos
{
    public class AnalysisDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateAnalysis { get; set; }
        public DateTime Date { get => DateAnalysis; set => DateAnalysis = value; }
        public string ContentType { get; set; } = string.Empty;
        public string ContentInput { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string ToolUsed { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SummaryResult { get; set; } = string.Empty;
        public string ResultJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int? DurationMs { get; set; }
        public string WcagVersion { get; set; } = string.Empty;
        public string WcagLevel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}