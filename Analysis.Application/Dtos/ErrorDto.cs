namespace Analysis.Application.Dtos
{
    public class ErrorDto
    {
        public int Id { get; set; }
        public int ResultId { get; set; }
        public int WcagCriterionId { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}