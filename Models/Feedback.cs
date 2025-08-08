namespace HR.Models
{
    public class FeedbackRequest
    {
        public int Employee_ID { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? GivenBy { get; set; }
        public DateTime? DateGiven { get; set; }
    }

    public class FeedbackResponse
    {
        public int Feedback_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DateGiven { get; set; }
        public string? GivenBy { get; set; }
    }
}
