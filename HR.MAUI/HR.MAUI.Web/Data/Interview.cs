using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Interview
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Interview_ID { get; set; }
        [ForeignKey(nameof(Application))]
        public int Application_ID { get; set; }
        public Application? Application { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Interviewer { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Scheduled, Completed, etc.
        // Soft delete and audit fields
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
