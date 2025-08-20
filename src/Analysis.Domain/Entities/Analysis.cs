using System;
using System.Collections.Generic;

namespace Analysis.Domain.Entities
{
    public class Analysis
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateAnalysis { get; set; }
        public ContentType ContentType { get; set; }
        public required string ContentInput { get; set; }
        public required string SourceUrl { get; set; }
        public ToolUsed ToolUsed { get; set; }
        public AnalysisStatus Status { get; set; }
        public required string SummaryResult { get; set; }
        public required string ResultJson { get; set; }
        public required string ErrorMessage { get; set; }
        public int? DurationMs { get; set; }
        public required string WcagVersion { get; set; }
        public WcagLevel WcagLevel { get; set; }
        public int? AxeViolations { get; set; }
        public int? AxeNeedsReview { get; set; }
        public int? AxeRecommendations { get; set; }
        public int? AxePasses { get; set; }
        public int? AxeIncomplete { get; set; }
        public int? AxeInapplicable { get; set; }
        public int? EaViolations { get; set; }
        public int? EaNeedsReview { get; set; }
        public int? EaRecommendations { get; set; }
        public int? EaPasses { get; set; }
        public int? EaIncomplete { get; set; }
        public int? EaInapplicable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public required ICollection<Result> Results { get; set; }
    }
}