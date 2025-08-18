using System;
using System.Collections.Generic;

namespace Analysis.Domain.Entities
{
    public class Result
    {
        public int Id { get; set; }
        public int AnalysisId { get; set; }
        public int WcagCriterionId { get; set; }
        public string WcagCriterion { get; set; }
        public ResultLevel Level { get; set; }
        public Severity Severity { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Analysis Analysis { get; set; }
        public ICollection<Error> Errors { get; set; }
    }
}