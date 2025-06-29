using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Incident
    {
        [Key]
        public int Incident_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int? Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string Status { get; set; } = string.Empty; // Open, Closed, etc.
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
