namespace Analysis.Domain.Entities;

public class Error
{
    public int Id { get; set; }
    public int ResultId { get; set; }
    public int WcagCriterionId { get; set; }
    public required string ErrorCode { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public required Result Result { get; set; }
}
