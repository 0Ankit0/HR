namespace HR.Models
{
    public class InterviewRequest
    {
        public int Application_ID { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Interviewer { get; set; } = string.Empty;
    }

    public class InterviewResponse
    {
        public int Interview_ID { get; set; }
        public int Application_ID { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Interviewer { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
