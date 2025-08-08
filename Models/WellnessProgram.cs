namespace HR.Models
{
    public class WellnessProgramRequest
    {
        public string ProgramName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ProgramType { get; set; } = string.Empty;
        public string Status { get; set; } = "Upcoming";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Duration { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; } = 50;
        public string? Benefits { get; set; }
    }

    public class WellnessProgramResponse
    {
        public int WellnessProgram_ID { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ProgramType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Duration { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; }
        public int ParticipantCount { get; set; }
        public string? Benefits { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class WellnessProgramListResponse
    {
        public List<WellnessProgramResponse> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
