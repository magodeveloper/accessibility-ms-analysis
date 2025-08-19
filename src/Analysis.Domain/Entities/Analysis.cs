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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public required ICollection<Result> Results { get; set; }
    }
}