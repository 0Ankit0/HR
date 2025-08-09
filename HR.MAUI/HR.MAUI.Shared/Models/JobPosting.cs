namespace HR.Models
{
    public class JobPostingRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal? Salary { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class JobPostingResponse
    {
        public int JobPosting_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal? Salary { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int ApplicationCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class JobPostingStatusUpdateRequest
    {
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
