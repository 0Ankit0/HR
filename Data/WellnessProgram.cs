using System;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class WellnessProgram
    {
        [Key]
        public int WellnessProgram_ID { get; set; }
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
        public int ParticipantCount { get; set; } = 0;
        public string? Benefits { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
