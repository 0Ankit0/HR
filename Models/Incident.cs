namespace HR.Models
{
    public class IncidentRequest
    {
        public int? Incident_ID { get; set; }
        public int ReportedBy { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string IncidentType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ActionsTaken { get; set; }
    }

    public class IncidentResponse
    {
        public int Incident_ID { get; set; }
        public int ReportedBy { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public DateTime? IncidentDate { get; set; }
        public string IncidentType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ActionsTaken { get; set; }
        public DateTime DateReported { get; set; }
    }

    public class IncidentListResponse
    {
        public List<IncidentResponse> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
