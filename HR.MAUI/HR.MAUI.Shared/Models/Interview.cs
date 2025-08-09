namespace HR.Models
{
    public class InterviewRequest
    {
        public int Application_ID { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public string Interviewer { get; set; } = string.Empty;
        public string InterviewType { get; set; } = "Video"; // Phone, Video, In-Person, Panel
        public string Notes { get; set; } = string.Empty;
    }

    public class InterviewResponse
    {
        public int Interview_ID { get; set; }
        public int Application_ID { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public string Interviewer { get; set; } = string.Empty;
        public string InterviewType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class InterviewStatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
