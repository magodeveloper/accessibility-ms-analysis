using System;

namespace Analysis.Domain.Entities
{
    public class Error
    {
        public int Id { get; set; }
        public int ResultId { get; set; }
        public int WcagCriterionId { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Result Result { get; set; }
    }
}