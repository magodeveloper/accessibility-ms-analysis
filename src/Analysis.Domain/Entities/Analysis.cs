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
        public string ContentInput { get; set; }
        public string SourceUrl { get; set; }
        public ToolUsed ToolUsed { get; set; }
        public AnalysisStatus Status { get; set; }
        public string SummaryResult { get; set; }
        public string ResultJson { get; set; }
        public string ErrorMessage { get; set; }
        public int? DurationMs { get; set; }
        public string WcagVersion { get; set; }
        public WcagLevel WcagLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Result> Results { get; set; }
    }
}