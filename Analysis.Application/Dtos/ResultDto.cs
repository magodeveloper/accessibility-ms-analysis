namespace Analysis.Application.Dtos
{
    public class ResultDto
    {
        public int Id { get; set; }
        public int AnalysisId { get; set; }
        public int WcagCriterionId { get; set; }
        public string WcagCriterion { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Summary { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}