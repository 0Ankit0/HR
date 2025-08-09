namespace HR.Models
{
    public class ApplicationRequest
    {
        public int JobPosting_ID { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
    }

    public class ApplicationStatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class ApplicationResponse
    {
        public int Application_ID { get; set; }
        public int JobPosting_ID { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class Application
    {
        public ApplicationRequest Request { get; set; } = new ApplicationRequest();
        public ApplicationResponse Response { get; set; } = new ApplicationResponse();
    }
}
