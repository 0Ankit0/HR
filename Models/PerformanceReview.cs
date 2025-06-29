namespace HR.Models
{
    public class PerformanceReviewRequest
    {
        public int Employee_ID { get; set; }
        public string Reviewer { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public int Score { get; set; }
    }

    public class PerformanceReviewResponse
    {
        public int PerformanceReview_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Reviewer { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public int Score { get; set; }
    }
}
