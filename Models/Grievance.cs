namespace HR.Models
{
    public class GrievanceRequest
    {
        public int? Grievance_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public DateTime? DateSubmitted { get; set; }
    }

    public class GrievanceResponse
    {
        public int Grievance_ID { get; set; }
        public int Employee_ID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? DateResolved { get; set; }
        public DateTime SubmittedDate { get; set; }
    }

    public class GrievanceListResponse
    {
        public List<GrievanceResponse> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
