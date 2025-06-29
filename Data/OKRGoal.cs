using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class OKRGoal
    {
        [Key]
        public int OKRGoal_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        [Required]
        public string Objective { get; set; } = string.Empty;
        [Required]
        public string KeyResults { get; set; } = string.Empty; // Could be JSON or delimited string
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
