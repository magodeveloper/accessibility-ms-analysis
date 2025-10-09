namespace Analysis.Domain.Entities;

public class Result
{
    public int Id { get; set; }
    public int AnalysisId { get; set; }
    public int WcagCriterionId { get; set; }
    public required string WcagCriterion { get; set; }
    public ResultLevel Level { get; set; }
    public Severity Severity { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public required Analysis Analysis { get; set; }
    public required ICollection<Error> Errors { get; set; }
}
