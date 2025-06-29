namespace HR.Models
{
    public class JobPostingRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class JobPostingResponse
    {
        public int JobPosting_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PostedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
